using System.Text.Json.Serialization;

namespace LithiumKit.Models;

/// <summary>
/// A Wine-compatible engine that powers a bottle. Whisky shipped a single
/// macOS toolchain (GPTK / Wine); Lithium is cross-platform, so an engine is
/// any wine/wine64 toolchain the OS backend can locate or the user points to.
/// </summary>
public sealed class WineEngine
{
    public WineEngine() { }

    public WineEngine(string id, string name, string? wineExecutablePath = null, string? flavor = null)
    {
        Id = id;
        Name = name;
        WineExecutablePath = wineExecutablePath;
        Flavor = flavor;
    }

    /// <summary>Stable identifier, e.g. "wine", "gptk", "cx23", "custom".</summary>
    public string Id { get; set; } = "wine";

    /// <summary>Human-friendly display name shown in the UI.</summary>
    public string Name { get; set; } = "Wine";

    /// <summary>Engine line: "wine", "gptk", "crossover", "proton", "custom".</summary>
    public string? Flavor { get; set; }

    /// <summary>Absolute path to the wine/wine64 executable. Null = resolved by the backend.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? WineExecutablePath { get; set; }

    /// <summary>Optional root directory of the engine toolchain (contains bin/).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolchainRoot { get; set; }

    public override string ToString() => Name;
}
