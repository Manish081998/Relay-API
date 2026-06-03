using Microsoft.Extensions.Options;
using Relay.Api.Settings;
using Relay.Intranet.Application.Abstractions;

namespace Relay.Api.Services;

public sealed class StagingFileWriter : IStagingFileWriter
{
    private readonly string _stagingDir;

    public StagingFileWriter(IOptions<FileStorageSettings> settings)
    {
        _stagingDir = settings?.Value.StagingDir ?? throw new ArgumentNullException(nameof(settings));
    }

    public Task WriteAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_stagingDir, fileName);
        return File.WriteAllTextAsync(path, content, cancellationToken);
    }

    public Task<string> ReadAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_stagingDir, fileName);
        return File.ReadAllTextAsync(path, cancellationToken);
    }
}
