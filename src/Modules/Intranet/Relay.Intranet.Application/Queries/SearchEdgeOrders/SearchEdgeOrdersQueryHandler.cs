using Relay.Intranet.Application.Mappers;
using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.SearchEdgeOrders;

internal sealed class SearchEdgeOrdersQueryHandler : IQueryHandler<SearchEdgeOrdersQuery, IReadOnlyList<EdgeOrderDto>>
{
    private readonly IEdgeOrderRepository _orders;

    public SearchEdgeOrdersQueryHandler(IEdgeOrderRepository orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    public async Task<Result<IReadOnlyList<EdgeOrderDto>>> HandleAsync(
        SearchEdgeOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var items = await _orders.SearchAsync(query.EmailId, query.ReleaseNumber, query.RepPO, query.PcUserName, query.RecordedDate, query.ReleaseName, cancellationToken);

        return Result.Success<IReadOnlyList<EdgeOrderDto>>(items.Select(o => o.ToDto()).ToList());
    }
}
