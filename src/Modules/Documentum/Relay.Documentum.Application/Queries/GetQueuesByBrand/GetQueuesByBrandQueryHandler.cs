using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetQueuesByBrand;

public sealed class GetQueuesByBrandQueryHandler : IQueryHandler<GetQueuesByBrandQuery, IReadOnlyList<string>>
{
    private readonly IEdgeOrderRepository _orders;

    public GetQueuesByBrandQueryHandler(IEdgeOrderRepository orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    public async Task<Result<IReadOnlyList<string>>> HandleAsync(
        GetQueuesByBrandQuery query, CancellationToken cancellationToken = default)
    {
        var queues = await _orders.GetQueuesByBrandAsync(query.BrandName, cancellationToken);
        return Result.Success(queues);
    }
}
