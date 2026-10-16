using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LithiumKit.Constants;
using LithiumKit.Models;
using LithiumKit.Services;

namespace Lithium.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly BackendRegistry _backends;
    private readonly EngineManager _engines;
    private readonly BottleManager _bottles;
    private readonly IWineRunner _runner;

    [ObservableProperty] private Bottle? _selectedBottle;
    [ObservableProperty] private string _statusText = "Ready.";
    [ObservableProperty] private bool _isBusy;

    public MainViewModel()
    {
        _backends = new BackendRegistry();
        _engines = new EngineManager(_backends);
        _bottles = new BottleManager();
        _runner = new WineRunner(_backends.Current, new ProcessRunner());
        Bottles = new ObservableCollection<Bottle>();
        Refresh();
    }

    public ObservableCollection<Bottle> Bottles { get; }

    public string AppTitle => LithiumConstants.AppNameFull;
    public string BackendName => _backends.Current.DisplayName;
    public IReadOnlyList<WineEngine> Engines => _engines.Engines;

    public bool HasSelection => SelectedBottle is not null;
    public bool ShowEmpty => !HasSelection;

    partial void OnSelectedBottleChanged(Bottle? value)
    {
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(ShowEmpty));
    }

    [RelayCommand]
    private void Refresh()
    {
        _bottles.Refresh();
        Bottles.Clear();
        foreach (var b in _bottles.Bottles.OrderBy(b => b.Name, StringComparer.OrdinalIgnoreCase))
            Bottles.Add(b);
        StatusText = $"{Bottles.Count} bottle(s) on {BackendName}.";
    }

    public Bottle CreateBottle(string name, WineEngine engine)
    {
        var bottle = _bottles.CreateBottle(name, engine);
        Refresh();
        return bottle;
    }

    public void DeleteBottle(Bottle bottle)
    {
        _bottles.DeleteBottle(bottle);
        if (SelectedBottle == bottle)
            SelectedBottle = null;
        Refresh();
    }

    public void AddProgramTo(Bottle bottle, string executablePath, string? name = null)
    {
        var program = new BottleProgram(name ?? Path.GetFileNameWithoutExtension(executablePath), executablePath);
        _bottles.AddProgram(bottle, program);
        StatusText = $"Added {program.Name} to {bottle.Name}.";
    }

    public void RemoveProgramFrom(Bottle bottle, BottleProgram program)
    {
        _bottles.RemoveProgram(bottle, program);
    }

    [RelayCommand]
    private async Task RunProgramAsync(BottleProgram? program)
    {
        if (program is null || SelectedBottle is null)
            return;

        var engine = ResolveEngine(SelectedBottle);
        IsBusy = true;
        StatusText = $"Launching {program.Name}…";
        try
        {
            var result = await _runner.RunProgramAsync(SelectedBottle, engine, program.Path, program.Arguments);
            StatusText = result.ExitCode == 0
                ? $"{program.Name} exited cleanly."
                : $"{program.Name} exited with code {result.ExitCode}.";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to launch {program.Name}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task InitializeBottleAsync()
    {
        if (SelectedBottle is null)
            return;

        var engine = ResolveEngine(SelectedBottle);
        IsBusy = true;
        StatusText = $"Initializing {SelectedBottle.Name}…";
        try
        {
            var result = await _runner.InitializeBottleAsync(SelectedBottle, engine);
            StatusText = result.ExitCode == 0
                ? $"{SelectedBottle.Name} initialized."
                : $"wineboot exited with code {result.ExitCode}.";
        }
        catch (Exception ex)
        {
            StatusText = $"Init failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private WineEngine ResolveEngine(Bottle bottle)
    {
        var id = bottle.Config?.EngineId;
        return (id is not null ? _engines.GetById(id) : null) ?? _engines.Engines[0];
    }
}
