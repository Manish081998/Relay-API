using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.SearchEdgeOrders;

public sealed record SearchEdgeOrdersQuery(
    string? SalesOrderNumber,
    string? RepPO,
    string? AccountNumber,
    string? ProductType,
    string? Region,
    string? Priority,
    string? Brand,
    DateTime? CaptureDateFrom,
    DateTime? CaptureDateTo,
    string? JobName,
    string? QueueName,
    string? PackageOwner,
    string? RepName,
    string? SortField,
    string? SortDirection,
    int PageNumber,
    int PageSize) : IQuery<PagedResultDto<EdgeOrderDto>>;
