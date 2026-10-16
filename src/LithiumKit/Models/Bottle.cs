using System.IO;
using LithiumKit.Constants;

namespace LithiumKit.Models;

/// <summary>A Wine prefix ("bottle") managed by Lithium.</summary>
public sealed class Bottle
{
    public Bottle() { }

    public Bottle(string path)
    {
        Path = path;
        Name = new DirectoryInfo(path).Name;
    }

    /// <summary>Absolute path to the bottle directory (== the Wine prefix).</summary>
    public string Path { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public BottleConfig? Config { get; set; }

    public List<BottleProgram> Programs { get; set; } = new();

    public string ConfigFilePath => System.IO.Path.Combine(Path, LithiumConstants.ConfigFileName);

    public string ProgramsFilePath => System.IO.Path.Combine(Path, LithiumConstants.ProgramsFileName);

    public BottleState State
    {
        get
        {
            if (string.IsNullOrEmpty(Path) || !Directory.Exists(Path))
                return BottleState.Broken;
            if (Config is null)
                return BottleState.NeedsSetup;
            return BottleState.Ready;
        }
    }
}
