namespace Relay.Infrastructure.Core.Data;

public sealed class ConnectionStringsSettings
{
    public string Intranet   { get; set; } = string.Empty;
    public string Documentum { get; set; } = string.Empty;
    public string WebTool  { get; set; } = string.Empty;
    public string Krueger { get; set; } = string.Empty;
    public string Titus { get; set; } = string.Empty;
    public string TNB { get; set; } = string.Empty;

    public string EdgeOrders { get; set; } = string.Empty;
}
