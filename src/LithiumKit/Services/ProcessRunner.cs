using System.Diagnostics;
using System.Text;

namespace LithiumKit.Services;

public sealed class ProcessResult
{
    public int ExitCode { get; init; }
    public string StandardOutput { get; init; } = string.Empty;
    public string StandardError { get; init; } = string.Empty;
    public bool TimedOut { get; init; }
}

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        IDictionary<string, string>? environment = null,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default);
}

/// <summary>Runs external processes (wine, wineboot, ...) and captures output.</summary>
public sealed class ProcessRunner : IProcessRunner
{
    public async Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        IDictionary<string, string>? environment = null,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory ?? string.Empty,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var a in arguments)
            psi.ArgumentList.Add(a);

        if (environment is not null)
        {
            foreach (var kv in environment)
                psi.Environment[kv.Key] = kv.Value;
        }

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        process.OutputDataReceived += (_, e) => { if (e.Data is not null) stdout.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) stderr.AppendLine(e.Data); };

        if (!process.Start())
            return new ProcessResult { ExitCode = -1, StandardError = "Failed to start process." };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill();
            return new ProcessResult { ExitCode = -1, TimedOut = true, StandardOutput = stdout.ToString(), StandardError = stderr.ToString() };
        }

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = stdout.ToString(),
            StandardError = stderr.ToString(),
        };
    }
}
