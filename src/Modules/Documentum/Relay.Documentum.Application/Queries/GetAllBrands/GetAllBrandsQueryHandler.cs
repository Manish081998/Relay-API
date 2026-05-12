using Relay.Documentum.Application.Mappers;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetAllBrands;

public sealed class GetAllBrandsQueryHandler : IQueryHandler<GetAllBrandsQuery, IReadOnlyList<BrandDto>>
{
    private readonly IBrandRepository _brands;

    public GetAllBrandsQueryHandler(IBrandRepository brands)
    {
        _brands = brands ?? throw new ArgumentNullException(nameof(brands));
    }

    public async Task<Result<IReadOnlyList<BrandDto>>> HandleAsync(
        GetAllBrandsQuery query, CancellationToken cancellationToken = default)
    {
        var brands = await _brands.GetAllAsync(cancellationToken);
        return Result.Success(brands.Select(b => b.ToDto()).ToList() as IReadOnlyList<BrandDto>);
    }
}
