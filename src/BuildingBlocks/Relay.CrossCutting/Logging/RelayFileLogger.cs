using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Relay.CrossCutting.Logging;

/// <summary>
/// Formats a log entry and enqueues it into the shared channel.
/// TryWrite is lock-free — the background writer is the only thread that touches the file.
/// </summary>
internal sealed class RelayFileLogger : ILogger
{
    private readonly string _category;
    private readonly ChannelWriter<string> _channel;
    private readonly RelayFileLoggerOptions _options;

    internal RelayFileLogger(string category, ChannelWriter<string> channel, RelayFileLoggerOptions options)
    {
        _category = category;
        _channel  = channel;
        _options  = options;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel != LogLevel.None && logLevel >= _options.MinimumLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception is null) return;

        _channel.TryWrite(BuildEntry(logLevel, message, exception));
    }

    private string BuildEntry(LogLevel logLevel, string message, Exception? exception)
    {
        var sb = new StringBuilder();
        sb.Append($"[{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC]");
        sb.Append($" [{ToShortLevel(logLevel)}]");
        sb.Append($" [{_category}]");
        sb.Append($" {message}");

        if (exception is not null)
        {
            sb.AppendLine();
            sb.Append($"  >> {exception}");
        }

        return sb.ToString();
    }

    private static string ToShortLevel(LogLevel level) => level switch
    {
        LogLevel.Trace       => "TRC",
        LogLevel.Debug       => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning     => "WRN",
        LogLevel.Error       => "ERR",
        LogLevel.Critical    => "CRT",
        _                    => "UNK"
    };
}

internal sealed class NullScope : IDisposable
{
    internal static readonly NullScope Instance = new();
    private NullScope() { }
    public void Dispose() { }
}
