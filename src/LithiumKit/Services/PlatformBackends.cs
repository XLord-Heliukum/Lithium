using System.Runtime.InteropServices;
using LithiumKit.Models;

namespace LithiumKit.Services;

/// <summary>Shared backend logic; OS specifics live in the subclasses.</summary>
public abstract class PlatformBackendBase : IPlatformBackend
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public abstract bool IsAvailable { get; }

    public virtual string? ResolveWineExecutable(WineEngine engine)
    {
        if (!string.IsNullOrEmpty(engine.WineExecutablePath) && File.Exists(engine.WineExecutablePath))
            return engine.WineExecutablePath;

        if (!string.IsNullOrEmpty(engine.ToolchainRoot))
        {
            var candidate = Path.Combine(engine.ToolchainRoot, "bin", "wine");
            if (File.Exists(candidate)) return candidate;
            var candidate64 = Path.Combine(engine.ToolchainRoot, "bin", "wine64");
            if (File.Exists(candidate64)) return candidate64;
        }

        return BackendHelpers.Which("wine") ?? BackendHelpers.Which("wine64");
    }

    public virtual IDictionary<string, string> BuildEnvironment(Bottle bottle, WineEngine engine)
    {
        var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["WINEPREFIX"] = bottle.Path,
            ["WINEDEBUG"] = "-all",
        };

        if (!string.IsNullOrEmpty(engine.ToolchainRoot))
        {
            var bin = Path.Combine(engine.ToolchainRoot, "bin");
            env["PATH"] = bin + Path.PathSeparator + (Environment.GetEnvironmentVariable("PATH") ?? string.Empty);
        }

        if (bottle.Config?.RetinaMode == true)
            env["WINE_HIDPI"] = "1";

        if (bottle.Config?.Esync == true)
            env["WINEESYNC"] = "1";
        else
            env["WINEESYNC"] = "0";

        if (bottle.Config is not null)
        {
            foreach (var line in bottle.Config.EnvironmentVariables)
            {
                var idx = line.IndexOf('=');
                if (idx > 0)
                    env[line[..idx]] = line[(idx + 1)..];
            }
        }

        return env;
    }

    public abstract IEnumerable<WineEngine> DiscoverEngines();
}

/// <summary>macOS backend: GPTK / Wine / Crossover-style toolchains.</summary>
public sealed class MacOSBackend : PlatformBackendBase
{
    public override string Id => "macos";
    public override string DisplayName => "macOS (GPTK / Wine)";
    public override bool IsAvailable => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public override IEnumerable<WineEngine> DiscoverEngines()
    {
        var pathWine = BackendHelpers.Which("wine") ?? BackendHelpers.Which("wine64");
        if (pathWine is not null)
            yield return new WineEngine("wine", "Wine (Homebrew)", pathWine, "wine");

        // Apple Game Porting Toolkit default install location.
        var gptk = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library", "Application Support", "com.apple.gameplatform");
        if (Directory.Exists(gptk))
            yield return new WineEngine("gptk", "Game Porting Toolkit", null, "gptk") { ToolchainRoot = gptk };

        // Common Crossover-like / Whisky-style toolchain roots.
        var crossover = "/Applications/CrossOver.app/Contents/SharedSupport/CrossOverEngine";
        if (Directory.Exists(crossover))
            yield return new WineEngine("crossover", "CrossOver", null, "crossover") { ToolchainRoot = crossover };
    }
}

/// <summary>Linux backend: distribution Wine / Proton.</summary>
public sealed class LinuxBackend : PlatformBackendBase
{
    public override string Id => "linux";
    public override string DisplayName => "Linux (Wine / Proton)";
    public override bool IsAvailable => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public override IEnumerable<WineEngine> DiscoverEngines()
    {
        var pathWine = BackendHelpers.Which("wine") ?? BackendHelpers.Which("wine64");
        if (pathWine is not null)
            yield return new WineEngine("wine", "Wine (system)", pathWine, "wine");

        var pathProton = BackendHelpers.Which("proton");
        if (pathProton is not null)
            yield return new WineEngine("proton", "Proton", pathProton, "proton");

        // Steam Proton installations.
        var steam = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".steam", "steam", "steamapps", "common");
        if (Directory.Exists(steam))
        {
            foreach (var dir in Directory.GetDirectories(steam, "Proton*"))
                yield return new WineEngine("proton-" + Path.GetFileName(dir), "Proton (" + Path.GetFileName(dir) + ")", null, "proton")
                {
                    ToolchainRoot = dir,
                };
        }
    }
}

/// <summary>Windows backend: a Wine build can still be used to manage prefixes.</summary>
public sealed class WindowsBackend : PlatformBackendBase
{
    public override string Id => "windows";
    public override string DisplayName => "Windows (Wine)";
    public override bool IsAvailable => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public override IEnumerable<WineEngine> DiscoverEngines()
    {
        var pathWine = BackendHelpers.Which("wine") ?? BackendHelpers.Which("wine64");
        if (pathWine is not null)
            yield return new WineEngine("wine", "Wine", pathWine, "wine");

        var localWine = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Lithium", "Engines");
        if (Directory.Exists(localWine))
        {
            foreach (var dir in Directory.GetDirectories(localWine))
                yield return new WineEngine("custom-" + Path.GetFileName(dir), "Wine (" + Path.GetFileName(dir) + ")", null, "custom")
                {
                    ToolchainRoot = dir,
                };
        }
    }
}
