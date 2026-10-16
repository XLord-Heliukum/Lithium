using LithiumKit.Models;

namespace LithiumKit.Services;

public interface IWineRunner
{
    Task<ProcessResult> InitializeBottleAsync(Bottle bottle, WineEngine engine, CancellationToken ct = default);
    Task<ProcessResult> RunProgramAsync(Bottle bottle, WineEngine engine, string executablePath, string? arguments = null, CancellationToken ct = default);
    Task<ProcessResult> RunWineCommandAsync(Bottle bottle, WineEngine engine, string wineArguments, CancellationToken ct = default);
}

/// <summary>Drives a Wine-compatible engine through the active platform backend.</summary>
public sealed class WineRunner : IWineRunner
{
    private readonly IPlatformBackend _backend;
    private readonly IProcessRunner _processRunner;

    public WineRunner(IPlatformBackend backend, IProcessRunner processRunner)
    {
        _backend = backend;
        _processRunner = processRunner;
    }

    public async Task<ProcessResult> InitializeBottleAsync(Bottle bottle, WineEngine engine, CancellationToken ct = default)
    {
        var wine = _backend.ResolveWineExecutable(engine)
            ?? throw new InvalidOperationException("Could not locate a Wine executable for the selected engine.");
        var env = _backend.BuildEnvironment(bottle, engine);
        return await _processRunner.RunAsync(wine, new[] { "wineboot", "-i" }, env, bottle.Path, ct);
    }

    public async Task<ProcessResult> RunProgramAsync(Bottle bottle, WineEngine engine, string executablePath, string? arguments = null, CancellationToken ct = default)
    {
        var wine = _backend.ResolveWineExecutable(engine)
            ?? throw new InvalidOperationException("Could not locate a Wine executable for the selected engine.");
        var args = new List<string> { executablePath };
        if (!string.IsNullOrWhiteSpace(arguments))
            args.AddRange(arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var env = _backend.BuildEnvironment(bottle, engine);
        return await _processRunner.RunAsync(wine, args, env, bottle.Path, ct);
    }

    public async Task<ProcessResult> RunWineCommandAsync(Bottle bottle, WineEngine engine, string wineArguments, CancellationToken ct = default)
    {
        var wine = _backend.ResolveWineExecutable(engine)
            ?? throw new InvalidOperationException("Could not locate a Wine executable for the selected engine.");
        var args = (wineArguments ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        var env = _backend.BuildEnvironment(bottle, engine);
        return await _processRunner.RunAsync(wine, args, env, bottle.Path, ct);
    }
}
