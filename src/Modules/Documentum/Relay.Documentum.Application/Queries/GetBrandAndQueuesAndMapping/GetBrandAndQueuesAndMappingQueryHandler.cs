using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetBrandAndQueuesAndMapping;

public sealed class GetBrandAndQueuesAndMappingQueryHandler
    : IQueryHandler<GetBrandAndQueuesAndMappingQuery, BrandAndQueuesAndMappingDto>
{
    private readonly IBrandMappingQueries _brandMappingQueries;

    public GetBrandAndQueuesAndMappingQueryHandler(IBrandMappingQueries brandMappingQueries)
    {
        _brandMappingQueries = brandMappingQueries ?? throw new ArgumentNullException(nameof(brandMappingQueries));
    }

    public async Task<Result<BrandAndQueuesAndMappingDto>> HandleAsync(
        GetBrandAndQueuesAndMappingQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _brandMappingQueries.GetBrandAndQueuesAndMappingAsync(cancellationToken);
        return Result.Success(result);
    }
}
