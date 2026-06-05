namespace Relay.Intranet.Application.Abstractions;

public interface IStagingFileWriter
{
    Task WriteAsync(string fileName, string content, CancellationToken cancellationToken = default);

    public Task WriteWorkingAsync(string fileName, string content, CancellationToken cancellationToken = default);

    Task<string> ReadAsync(string fileName, CancellationToken cancellationToken = default);

    public Task<string> ReadWorkingAsync(string fileName, CancellationToken cancellationToken = default);

    public Task<bool> DeleteFile(string filename, CancellationToken cancellationToken = default);
}
