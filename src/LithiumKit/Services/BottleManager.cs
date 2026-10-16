using System.Text.Json;
using LithiumKit.Constants;
using LithiumKit.Models;

namespace LithiumKit.Services;

public interface IBottleManager
{
    IReadOnlyList<Bottle> Bottles { get; }
    void Refresh();
    Bottle CreateBottle(string name, WineEngine engine, BottleConfig? config = null);
    void DeleteBottle(Bottle bottle);
    void LoadConfig(Bottle bottle);
    void SaveConfig(Bottle bottle);
    void AddProgram(Bottle bottle, BottleProgram program);
    void RemoveProgram(Bottle bottle, BottleProgram program);
    void SavePrograms(Bottle bottle);
}

/// <summary>Creates, lists, persists and runs Wine bottles (prefixes).</summary>
public sealed class BottleManager : IBottleManager
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _bottlesRoot;
    private readonly List<Bottle> _bottles = new();

    public BottleManager(string? bottlesRoot = null)
    {
        _bottlesRoot = bottlesRoot ?? SettingsManager.BottlesDirectory();
        Directory.CreateDirectory(_bottlesRoot);
    }

    public IReadOnlyList<Bottle> Bottles => _bottles;

    public void Refresh()
    {
        _bottles.Clear();
        foreach (var dir in Directory.GetDirectories(_bottlesRoot))
        {
            var bottle = new Bottle(dir);
            LoadConfig(bottle);
            LoadPrograms(bottle);
            _bottles.Add(bottle);
        }
    }

    public Bottle CreateBottle(string name, WineEngine engine, BottleConfig? config = null)
    {
        var safe = Sanitize(name);
        var path = Path.Combine(_bottlesRoot, safe);
        Directory.CreateDirectory(path);
        var bottle = new Bottle(path)
        {
            Config = config ?? new BottleConfig { Name = name, EngineId = engine.Id },
        };
        bottle.Config.Name = name;
        bottle.Config.EngineId = engine.Id;
        SaveConfig(bottle);
        SavePrograms(bottle);
        _bottles.Add(bottle);
        return bottle;
    }

    public void DeleteBottle(Bottle bottle)
    {
        if (Directory.Exists(bottle.Path))
            Directory.Delete(bottle.Path, recursive: true);
        _bottles.Remove(bottle);
    }

    public void LoadConfig(Bottle bottle)
    {
        if (File.Exists(bottle.ConfigFilePath))
        {
            try
            {
                bottle.Config = JsonSerializer.Deserialize<BottleConfig>(File.ReadAllText(bottle.ConfigFilePath));
            }
            catch
            {
                bottle.Config = null;
            }
        }

        if (bottle.Config is not null && string.IsNullOrEmpty(bottle.Config.Name))
            bottle.Config.Name = bottle.Name;
    }

    public void SaveConfig(Bottle bottle)
    {
        bottle.Config ??= new BottleConfig { Name = bottle.Name };
        File.WriteAllText(bottle.ConfigFilePath, JsonSerializer.Serialize(bottle.Config, JsonOptions));
    }

    public void AddProgram(Bottle bottle, BottleProgram program)
    {
        if (!bottle.Programs.Any(p => p.Path.Equals(program.Path, StringComparison.OrdinalIgnoreCase)))
            bottle.Programs.Add(program);
        SavePrograms(bottle);
    }

    public void RemoveProgram(Bottle bottle, BottleProgram program)
    {
        bottle.Programs.RemoveAll(p => p.Path.Equals(program.Path, StringComparison.OrdinalIgnoreCase));
        SavePrograms(bottle);
    }

    public void SavePrograms(Bottle bottle)
    {
        File.WriteAllText(bottle.ProgramsFilePath, JsonSerializer.Serialize(bottle.Programs, JsonOptions));
    }

    private void LoadPrograms(Bottle bottle)
    {
        if (File.Exists(bottle.ProgramsFilePath))
        {
            try
            {
                bottle.Programs = JsonSerializer.Deserialize<List<BottleProgram>>(File.ReadAllText(bottle.ProgramsFilePath)) ?? new List<BottleProgram>();
            }
            catch
            {
                bottle.Programs = new List<BottleProgram>();
            }
        }
    }

    private static string Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        return string.IsNullOrEmpty(cleaned) ? "Bottle" : cleaned;
    }
}
