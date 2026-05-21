using Relay.Documentum.Application.Mappers;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.SearchEdgeOrders;

public sealed class SearchEdgeOrdersQueryHandler : IQueryHandler<SearchEdgeOrdersQuery, PagedResultDto<EdgeOrderDto>>
{
    private readonly IEdgeOrderRepository _orders;

    public SearchEdgeOrdersQueryHandler(IEdgeOrderRepository orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    /// <summary>
    /// This method search the orders on the basis of request.
    /// </summary>
    /// <param name="query">Dynamic query with search field</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<PagedResultDto<EdgeOrderDto>>> HandleAsync(
        SearchEdgeOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _orders.SearchAsync(
            query.SalesOrderNumber,
            query.RepPO,
            query.AccountNumber,
            query.ProductType,
            query.Region,
            query.Priority,
            query.Brand,
            query.CaptureDateFrom,
            query.CaptureDateTo,
            query.JobName,
            query.QueueName,
            query.PackageOwner,
            query.RepName,
            query.SortField,
            query.SortDirection,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var pagedResult = new PagedResultDto<EdgeOrderDto>(
            items.Select(o => o.ToDto()).ToList(),
            totalCount,
            query.PageNumber,
            query.PageSize);

        return Result.Success(pagedResult);
    }
}
