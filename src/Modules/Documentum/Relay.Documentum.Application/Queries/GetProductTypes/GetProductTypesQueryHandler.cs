using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetProductTypes;

public sealed class GetProductTypesQueryHandler : IQueryHandler<GetProductTypesQuery, IReadOnlyList<ProductTypeDto>>
{
    private readonly IEdgeOrderRepository _orders;

    public GetProductTypesQueryHandler(IEdgeOrderRepository orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    public async Task<Result<IReadOnlyList<ProductTypeDto>>> HandleAsync(
        GetProductTypesQuery query, CancellationToken cancellationToken = default)
    {
        var types = await _orders.GetProductTypesByBrandAsync(query.BrandName, cancellationToken);
        return Result.Success(types);
    }
}
