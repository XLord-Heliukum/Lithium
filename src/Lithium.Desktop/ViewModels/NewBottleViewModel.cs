using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LithiumKit.Models;

namespace Lithium.Desktop.ViewModels;

public partial class NewBottleViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private WineEngine? _selectedEngine;
    [ObservableProperty] private string _windowsVersion = "win10";
    [ObservableProperty] private bool _esync = true;
    [ObservableProperty] private bool _retinaMode = true;

    public NewBottleViewModel(IReadOnlyList<WineEngine> engines)
    {
        Engines = new ObservableCollection<WineEngine>(engines);
        SelectedEngine = engines.FirstOrDefault();
        WindowsVersions = new ObservableCollection<string> { "win7", "win8", "win10", "win11" };
    }

    public ObservableCollection<WineEngine> Engines { get; }
    public ObservableCollection<string> WindowsVersions { get; }

    public bool CanCreate => !string.IsNullOrWhiteSpace(Name) && SelectedEngine is not null;
}
