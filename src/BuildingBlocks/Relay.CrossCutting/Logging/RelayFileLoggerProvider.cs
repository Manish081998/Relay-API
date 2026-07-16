using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Relay.CrossCutting.Logging;

/// <summary>
/// Custom ILoggerProvider that writes to a rolling daily log file.
/// Uses a Channel as an in-memory queue so all threads enqueue entries lock-free.
/// A single background task is the only writer to the file — zero file contention
/// regardless of how many concurrent requests are running.
/// </summary>
[ProviderAlias("RelayFile")]
public sealed class RelayFileLoggerProvider : ILoggerProvider
{
    private readonly RelayFileLoggerOptions _options;
    private readonly Channel<string> _channel;
    private readonly Task _writerTask;
    private DateOnly _currentDate;
    private StreamWriter? _writer;
    private bool _disposed;

    public RelayFileLoggerProvider(RelayFileLoggerOptions options)
    {
        _options = options;

        _channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true });

        System.IO.Directory.CreateDirectory(options.Directory);

        _currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        _writer      = OpenWriter(_currentDate);

        _writerTask = Task.Factory.StartNew(
            WriteLoopAsync,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();
    }

    public ILogger CreateLogger(string categoryName) =>
        new RelayFileLogger(categoryName, _channel.Writer, _options);

    // ── Background writer ────────────────────────────────────────────────────

    private async Task WriteLoopAsync()
    {
        await foreach (var line in _channel.Reader.ReadAllAsync())
        {
            try
            {
                RollIfNeeded();
                _writer?.WriteLine(line);
                _writer?.Flush();
            }
            catch
            {
                // Never crash the loop — silently skip a bad write
            }
        }

        try { _writer?.Flush(); _writer?.Dispose(); } catch { }
        _writer = null;
    }

    private void RollIfNeeded()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today == _currentDate) return;

        _writer?.Flush();
        _writer?.Dispose();
        _currentDate = today;
        _writer      = OpenWriter(today);
        PurgeOldFiles();
    }

    // ── File helpers ─────────────────────────────────────────────────────────

    private StreamWriter OpenWriter(DateOnly date)
    {
        var path   = Path.Combine(_options.Directory, $"{_options.FilePrefix}-{date:yyyy-MM-dd}.log");
        var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
        return new StreamWriter(stream, System.Text.Encoding.UTF8, bufferSize: 4096, leaveOpen: false);
    }

    private void PurgeOldFiles()
    {
        try
        {
            var cutoff = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-_options.RetainedDays);
            var dir    = new DirectoryInfo(_options.Directory);
            if (!dir.Exists) return;

            foreach (var file in dir.GetFiles($"{_options.FilePrefix}-*.log"))
            {
                var datePart = Path.GetFileNameWithoutExtension(file.Name)
                    .Substring(_options.FilePrefix.Length + 1);

                if (DateOnly.TryParse(datePart, out var fileDate) && fileDate < cutoff)
                    file.Delete();
            }
        }
        catch
        {
            // Purge is best-effort
        }
    }

    // ── Disposal ─────────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _channel.Writer.Complete();          // no more writes
        _writerTask.GetAwaiter().GetResult(); // wait for full drain before process exits
    }
}
