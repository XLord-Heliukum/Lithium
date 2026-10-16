namespace LithiumKit.Models;

/// <summary>Persistent configuration for a single bottle (stored as bottle.json).</summary>
public sealed class BottleConfig
{
    public string Name { get; set; } = string.Empty;
    public string? EngineId { get; set; }
    public string WindowsVersion { get; set; } = "win10";
    public bool Esync { get; set; } = true;
    public bool Msync { get; set; }
    public bool RetinaMode { get; set; } = true;
    public int? WindowedWidth { get; set; }
    public int? WindowedHeight { get; set; }
    public List<string> EnvironmentVariables { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
