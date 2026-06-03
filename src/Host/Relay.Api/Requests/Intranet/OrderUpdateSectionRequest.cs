namespace Relay.Api.Requests.Intranet;

public class OrderUpdateSectionRequest
{
    public string Section { get; set; } = string.Empty;
    public Dictionary<string, string> Fields { get; set; } = new();
}
