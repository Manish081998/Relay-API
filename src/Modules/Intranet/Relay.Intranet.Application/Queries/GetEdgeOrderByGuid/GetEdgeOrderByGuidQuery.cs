using Relay.Intranet.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.GetEdgeOrderByGuid;

public sealed record GetEdgeOrderByGuidQuery(
    string OrderGuid,
    string RepPo) : IQuery<EdgeOrderDetailDto?>;
