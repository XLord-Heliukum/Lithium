namespace LithiumKit.Models;

/// <summary>A Windows executable registered inside a bottle.</summary>
public sealed class BottleProgram
{
    public BottleProgram() { }

    public BottleProgram(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Arguments { get; set; }
    public string? WorkingDirectory { get; set; }
    public bool IsWineUtility { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
