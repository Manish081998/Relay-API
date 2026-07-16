namespace Relay.Api.Requests.Intranet;

public class PlantCodeUpdateDto
{
    public string LineNumber { get; set; } = string.Empty;
    public string NewPlantCode { get; set; } = string.Empty;
    public bool IsSecondaryPlant { get; set; }
}
