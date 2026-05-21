namespace Relay.Intranet.Contracts.Dtos;

public sealed record PagedEdgeOrderResultDto(
    IReadOnlyList<EdgeOrderDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
