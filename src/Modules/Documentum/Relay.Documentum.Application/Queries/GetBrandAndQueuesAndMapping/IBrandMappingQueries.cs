using Relay.Documentum.Contracts.Dtos;

namespace Relay.Documentum.Application.Queries.GetBrandAndQueuesAndMapping;

public interface IBrandMappingQueries
{
    Task<BrandAndQueuesAndMappingDto> GetBrandAndQueuesAndMappingAsync(CancellationToken cancellationToken = default);
}
