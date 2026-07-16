namespace Relay.Intranet.Contracts.Dtos;

public class OrderSectionUpdateDto
{
    public string Section { get; set; } = string.Empty;
    public Dictionary<string, string> Fields { get; set; } = new();
}
