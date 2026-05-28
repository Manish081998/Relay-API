using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetRegionsByBrand;

public sealed class GetRegionsByBrandQueryHandler : IQueryHandler<GetRegionsByBrandQuery, IReadOnlyList<RegionDto>>
{
    private readonly IEdgeOrderRepository _orders;

    public GetRegionsByBrandQueryHandler(IEdgeOrderRepository orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    public async Task<Result<IReadOnlyList<RegionDto>>> HandleAsync(
        GetRegionsByBrandQuery query, CancellationToken cancellationToken = default)
    {
        var regions = await _orders.GetRegionsByBrandAsync(query.BrandName, cancellationToken);
        return Result.Success(regions);
    }
}
