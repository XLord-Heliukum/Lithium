namespace LithiumKit.Services;

/// <summary>
/// Downloads Wine-compatible engine builds. On macOS this is where a Whisky-style
/// GPTK/Wine distribution would be fetched; on Linux, distribution packages are
/// preferred. Network access is optional and reports progress via a callback.
/// </summary>
public interface IDownloadManager
{
    Task<bool> DownloadEngineAsync(string url, string destination, IProgress<double>? progress = null, CancellationToken ct = default);
}

public sealed class DownloadManager : IDownloadManager
{
    public async Task<bool> DownloadEngineAsync(string url, string destination, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
            return false;

        var total = response.Content.Headers.ContentLength ?? -1L;
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        await using var file = File.Create(destination);

        var buffer = new byte[8192];
        long read = 0;
        int n;
        while ((n = await stream.ReadAsync(buffer, ct)) > 0)
        {
            await file.WriteAsync(buffer.AsMemory(0, n), ct);
            read += n;
            if (total > 0)
                progress?.Report((double)read / total);
        }

        return true;
    }
}
