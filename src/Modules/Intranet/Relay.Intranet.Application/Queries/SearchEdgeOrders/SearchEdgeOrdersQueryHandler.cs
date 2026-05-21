using Relay.Intranet.Application.Mappers;
using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.SearchEdgeOrders;

internal sealed class SearchEdgeOrdersQueryHandler : IQueryHandler<SearchEdgeOrdersQuery, PagedEdgeOrderResultDto>
{
    private readonly IEdgeOrderRepository _orders;

    public SearchEdgeOrdersQueryHandler(IEdgeOrderRepository orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    public async Task<Result<PagedEdgeOrderResultDto>> HandleAsync(
        SearchEdgeOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _orders.SearchAsync(
            query.EmailId, query.ReleaseNumber, query.RepPO,
            query.PcUserName, query.RecordedDate, query.ReleaseName,
            query.PageNumber, query.PageSize,
            cancellationToken);

        return Result.Success(new PagedEdgeOrderResultDto(
            items.Select(o => o.ToDto()).ToList(),
            totalCount,
            query.PageNumber,
            query.PageSize));
    }
}
