using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Domain.Repositories;

public interface IQueueRepository
{
    Task<IReadOnlyList<Queue>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> AddAsync(Queue queue, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(Queue queue, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(int queueId, CancellationToken cancellationToken = default);
    Task<BrandQueueMappingResultDto> GetBrandQueueMappingAsync(
        string? globalId, string? actionType, int brandId, string? queueId,
        CancellationToken cancellationToken = default);
}
