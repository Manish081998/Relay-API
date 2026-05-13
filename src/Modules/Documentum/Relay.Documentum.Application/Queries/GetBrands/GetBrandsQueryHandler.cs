using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetBrands;

public sealed class GetBrandsQueryHandler : IQueryHandler<GetBrandsQuery, IReadOnlyList<string>>
{
    private readonly IEdgeOrderRepository _orders;

    public GetBrandsQueryHandler(IEdgeOrderRepository orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    public async Task<Result<IReadOnlyList<string>>> HandleAsync(
        GetBrandsQuery query, CancellationToken cancellationToken = default)
    {
        var brands = await _orders.GetDistinctBrandsAsync(cancellationToken);
        return Result.Success(brands);
    }
}
