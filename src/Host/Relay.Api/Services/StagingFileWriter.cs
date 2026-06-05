using System.Threading;
using Microsoft.Extensions.Options;
using Relay.Api.Settings;
using Relay.Intranet.Application.Abstractions;

namespace Relay.Api.Services;

public sealed class StagingFileWriter : IStagingFileWriter
{
    private readonly string _stagingDir;
    private readonly string _WorkingDir;

    public StagingFileWriter(IOptions<FileStorageSettings> settings)
    {
        _stagingDir = settings?.Value.StagingDir ?? throw new ArgumentNullException(nameof(settings));
        _WorkingDir = settings?.Value.WorkingDir ?? throw new ArgumentNullException(nameof(settings));
    }

    public Task WriteAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_stagingDir, fileName);
        return File.WriteAllTextAsync(path, content, cancellationToken);
    }

    public Task WriteWorkingAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_WorkingDir, fileName);
        return File.WriteAllTextAsync(path, content, cancellationToken);
    }

    public async Task<string> ReadAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_stagingDir, fileName);

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public async Task<string> ReadWorkingAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_WorkingDir, fileName);
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public Task<bool> DeleteFile(string filename, CancellationToken cancellationToken = default)
    {

        cancellationToken.ThrowIfCancellationRequested();

        var path = Path.Combine(_stagingDir, filename);

        if (!File.Exists(path))
            return Task.FromResult(false);

        try
        {
            File.Delete(path);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }

    }
}
