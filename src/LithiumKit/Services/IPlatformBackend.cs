using LithiumKit.Models;

namespace LithiumKit.Services;

/// <summary>
/// Abstraction over the OS-specific way of locating and driving a Wine-compatible
/// toolchain. Whisky was macOS-only (GPTK/Wine); Lithium is cross-platform, so each
/// operating system provides its own backend implementation.
/// </summary>
public interface IPlatformBackend
{
    string Id { get; }
    string DisplayName { get; }
    bool IsAvailable { get; }

    /// <summary>Resolve the actual wine/wine64 executable for the given engine.</summary>
    string? ResolveWineExecutable(WineEngine engine);

    /// <summary>Build the environment block (WINEPREFIX, PATH, ...) used to launch the engine.</summary>
    IDictionary<string, string> BuildEnvironment(Bottle bottle, WineEngine engine);

    /// <summary>Discover engines that are present/installable on this OS.</summary>
    IEnumerable<WineEngine> DiscoverEngines();
}

internal static class BackendHelpers
{
    public static string? Which(string name)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var full = Path.Combine(dir, name);
                if (File.Exists(full))
                    return full;
            }
            catch
            {
                // ignore inaccessible PATH entries
            }
        }
        return null;
    }
}
