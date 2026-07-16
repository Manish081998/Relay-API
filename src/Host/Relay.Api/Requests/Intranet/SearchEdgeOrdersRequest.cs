namespace Relay.Api.Requests.Intranet;

public sealed record SearchEdgeOrdersRequest(
    string? EmailId = null,
    string? ReleaseNumber = null,
    string? RepPO = null,
    string? PcUserName = null,
    string? RecordedDate = null,
    string? ReleaseName = null,
    int PageNumber = 1,
    int PageSize = 50);
