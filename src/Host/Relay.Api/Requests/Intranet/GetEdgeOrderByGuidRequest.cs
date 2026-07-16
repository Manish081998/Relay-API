namespace Relay.Api.Requests.Intranet;

public sealed record GetEdgeOrderByGuidRequest(
    string UserId,
    Guid? OrderGuid = null,
    string? RepPo = null
    );
