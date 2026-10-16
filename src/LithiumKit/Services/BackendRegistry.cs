using System.Runtime.InteropServices;

namespace LithiumKit.Services;

/// <summary>Holds every platform backend and exposes the one matching the current OS.</summary>
public sealed class BackendRegistry
{
    private readonly List<IPlatformBackend> _backends = new();

    public BackendRegistry()
    {
        _backends.Add(new MacOSBackend());
        _backends.Add(new LinuxBackend());
        _backends.Add(new WindowsBackend());
    }

    public IReadOnlyList<IPlatformBackend> All => _backends;

    public IPlatformBackend Current =>
        _backends.FirstOrDefault(b => b.IsAvailable)
        ?? throw new PlatformNotSupportedException(
            $"Lithium has no backend for this platform ({RuntimeInformation.OSDescription}).");
}
