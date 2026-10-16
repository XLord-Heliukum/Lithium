using System.Text.Json;

namespace LithiumKit.Services;

public sealed class LithiumSettings
{
    public string? DefaultEngineId { get; set; }
    public string? PreferredWinePath { get; set; }
    public bool CheckForUpdates { get; set; } = true;
    public string? BottlesRootOverride { get; set; }
}

public interface ISettingsManager
{
    LithiumSettings Current { get; }
    void Load();
    void Save();
}

/// <summary>Persists global Lithium settings under the OS application-data folder.</summary>
public sealed class SettingsManager : ISettingsManager
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string SettingsPath = Path.Combine(BaseDirectory(), "settings.json");

    public LithiumSettings Current { get; private set; } = new();

    public SettingsManager() => Load();

    public void Load()
    {
        if (File.Exists(SettingsPath))
        {
            try
            {
                Current = JsonSerializer.Deserialize<LithiumSettings>(File.ReadAllText(SettingsPath)) ?? new LithiumSettings();
            }
            catch
            {
                Current = new LithiumSettings();
            }
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(Current, JsonOptions));
    }

    /// <summary>Root configuration directory, e.g. ~/Library/Application Support/Lithium (macOS).</summary>
    public static string BaseDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrEmpty(appData))
            appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "Lithium");
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>Directory that holds all bottle prefixes.</summary>
    public static string BottlesDirectory()
    {
        var dir = Path.Combine(BaseDirectory(), "Bottles");
        Directory.CreateDirectory(dir);
        return dir;
    }
}
