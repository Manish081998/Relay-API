using Relay.Intranet.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.SearchEdgeOrders;

public sealed record SearchEdgeOrdersQuery(
    string? EmailId,
    string? ReleaseNumber,
    string? RepPO,
    string? PcUserName,
    string? RecordedDate,
    string? ReleaseName) : IQuery<IReadOnlyList<EdgeOrderDto>>;
