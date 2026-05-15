using Relay.Documentum.Application.Mappers;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetEdgeOrderBySeq;

public sealed class GetEdgeOrderBySeqQueryHandler : IQueryHandler<GetEdgeOrderBySeqQuery, EdgeOrderDto?>
{
    private readonly IEdgeOrderRepository _orders;

    public GetEdgeOrderBySeqQueryHandler(IEdgeOrderRepository orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    public async Task<Result<EdgeOrderDto?>> HandleAsync(
        GetEdgeOrderBySeqQuery query, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByOrderSeqAsync(query.OrderSeq, cancellationToken);
        return Result.Success(order?.ToDto());
    }
}
