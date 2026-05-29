namespace Relay.Api.Requests.Intranet;

public sealed record GetEdgeOrderByGuidRequest(
    Guid? OrderGuid = null,
    string? RepPo = null);
