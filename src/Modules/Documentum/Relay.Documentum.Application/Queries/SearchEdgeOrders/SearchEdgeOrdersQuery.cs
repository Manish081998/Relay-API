using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.SearchEdgeOrders;

public sealed record SearchEdgeOrdersQuery(
    int? OrderSeq,
    string? RepPO,
    string? AccountNumber,
    string? Brand,
    DateTime? OrderDateFrom,
    DateTime? OrderDateTo,
    int PageNumber,
    int PageSize) : IQuery<PagedResultDto<EdgeOrderDto>>;
