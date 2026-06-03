namespace Relay.Intranet.Application.Abstractions;

public interface IStagingFileWriter
{
    Task WriteAsync(string fileName, string content, CancellationToken cancellationToken = default);
    Task<string> ReadAsync(string fileName, CancellationToken cancellationToken = default);
}
