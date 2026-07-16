using Microsoft.Extensions.Logging;

namespace Relay.CrossCutting.Logging;

public sealed class RelayFileLoggerOptions
{
    public string   Directory    { get; set; } = "logs";
    public string   FilePrefix   { get; set; } = "relay";
    public int      RetainedDays { get; set; } = 30;
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
}
