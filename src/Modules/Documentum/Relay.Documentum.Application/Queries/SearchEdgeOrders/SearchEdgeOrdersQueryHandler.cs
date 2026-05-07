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

    public async Task<Result<PagedResultDto<EdgeOrderDto>>> HandleAsync(
        SearchEdgeOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _orders.SearchAsync(
            query.OrderSeq,
            query.RepPO,
            query.AccountNumber,
            query.Brand,
            query.OrderDateFrom,
            query.OrderDateTo,
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
