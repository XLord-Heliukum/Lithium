using System;
using System.Linq;
using System.Threading.Tasks;
using LithiumKit.Constants;
using LithiumKit.Models;
using LithiumKit.Services;

namespace Lithium.Cli;

/// <summary>
/// Lithium command-line interface, the cross-platform successor to WhiskyCmd.
/// </summary>
internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 0;
        }

        var backends = new BackendRegistry();
        var engines = new EngineManager(backends);
        var bottles = new BottleManager();

        switch (args[0].ToLowerInvariant())
        {
            case "list-engines":
                Console.WriteLine($"Backend: {backends.Current.DisplayName}");
                foreach (var e in engines.Engines)
                    Console.WriteLine($"  {e.Id,-12} {e.Name} ({e.Flavor})");
                return 0;

            case "list":
            case "list-bottles":
                bottles.Refresh();
                Console.WriteLine($"{bottles.Bottles.Count} bottle(s):");
                foreach (var b in bottles.Bottles.OrderBy(b => b.Name, StringComparer.OrdinalIgnoreCase))
                    Console.WriteLine($"  {b.Name,-20} {b.State} engine={b.Config?.EngineId}");
                return 0;

            case "create":
                if (args.Length < 2)
                {
                    Console.Error.WriteLine("usage: lithium create <name> [engineId]");
                    return 1;
                }
                var engine = engines.Engines[0];
                if (args.Length > 2)
                    engine = engines.GetById(args[2]) ?? engine;
                var bottle = bottles.CreateBottle(args[1], engine);
                Console.WriteLine($"Created bottle '{bottle.Name}' at {bottle.Path}");
                return 0;

            case "delete":
                if (args.Length < 2)
                {
                    Console.Error.WriteLine("usage: lithium delete <name>");
                    return 1;
                }
                bottles.Refresh();
                var target = bottles.Bottles.FirstOrDefault(b => b.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase));
                if (target is null)
                {
                    Console.Error.WriteLine($"Bottle '{args[1]}' not found.");
                    return 1;
                }
                bottles.DeleteBottle(target);
                Console.WriteLine($"Deleted bottle '{target.Name}'.");
                return 0;

            case "run":
                if (args.Length < 3)
                {
                    Console.Error.WriteLine("usage: lithium run <bottleName> <exePath>");
                    return 1;
                }
                bottles.Refresh();
                var runBottle = bottles.Bottles.FirstOrDefault(b => b.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase));
                if (runBottle is null)
                {
                    Console.Error.WriteLine($"Bottle '{args[1]}' not found.");
                    return 1;
                }
                var runner = new WineRunner(backends.Current, new ProcessRunner());
                var runEngine = engines.GetById(runBottle.Config?.EngineId ?? engines.Engines[0].Id) ?? engines.Engines[0];
                var result = await runner.RunProgramAsync(runBottle, runEngine, args[2]);
                Console.Write(result.StandardOutput);
                if (!string.IsNullOrEmpty(result.StandardError))
                    Console.Error.Write(result.StandardError);
                return result.ExitCode;

            case "about":
                Console.WriteLine(LithiumConstants.AppNameFull);
                Console.WriteLine(LithiumConstants.Tagline);
                Console.WriteLine($"Upstream: {LithiumConstants.UpstreamProject} ({LithiumConstants.UpstreamUrl})");
                Console.WriteLine("License: GPL-3.0 (derivative work of Whisky)");
                return 0;

            default:
                PrintUsage();
                return 1;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Lithium CLI — a cross-platform Wine wrapper.");
        Console.WriteLine("Usage:");
        Console.WriteLine("  lithium list-engines");
        Console.WriteLine("  lithium list");
        Console.WriteLine("  lithium create <name> [engineId]");
        Console.WriteLine("  lithium delete <name>");
        Console.WriteLine("  lithium run <bottleName> <exePath>");
        Console.WriteLine("  lithium about");
    }
}
