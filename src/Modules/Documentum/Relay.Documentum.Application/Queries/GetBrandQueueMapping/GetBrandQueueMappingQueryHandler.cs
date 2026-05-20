using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetBrandQueueMapping;

public sealed class GetBrandQueueMappingQueryHandler : IQueryHandler<GetBrandQueueMappingQuery, BrandQueueMappingResultDto>
{
    private readonly IQueueRepository _queues;

    public GetBrandQueueMappingQueryHandler(IQueueRepository queues)
    {
        _queues = queues ?? throw new ArgumentNullException(nameof(queues));
    }

    public async Task<Result<BrandQueueMappingResultDto>> HandleAsync(
        GetBrandQueueMappingQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _queues.GetBrandQueueMappingAsync(
            query.GlobalId, query.ActionType, query.BrandId, query.QueueId, cancellationToken);

        return Result.Success(result);
    }
}
