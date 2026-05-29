using Relay.Intranet.Application.Mappers;
using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.GetEdgeOrderByGuid;

internal sealed class GetEdgeOrderByGuidQueryHandler : IQueryHandler<GetEdgeOrderByGuidQuery, EdgeOrderDetailDto?>
{
    private readonly IEdgeOrderRepository _orders;

    public GetEdgeOrderByGuidQueryHandler(IEdgeOrderRepository orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    public async Task<Result<EdgeOrderDetailDto?>> HandleAsync(
        GetEdgeOrderByGuidQuery query, CancellationToken cancellationToken = default)
    {
        var detail = await _orders.GetByOrderGuidAsync(
            query.OrderGuid, query.RepPo, cancellationToken);

        if (detail?.ErrorMessage is not null)
            return Result.Failure<EdgeOrderDetailDto?>(
                new AppError("Order.SpError", detail.ErrorMessage));

        return Result.Success<EdgeOrderDetailDto?>(detail?.ToDto());
    }
}
