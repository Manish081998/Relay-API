using Relay.Documentum.Application.Mappers;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetAllQueues;

public sealed class GetAllQueuesQueryHandler : IQueryHandler<GetAllQueuesQuery, IReadOnlyList<QueueDto>>
{
    private readonly IQueueRepository _queues;

    public GetAllQueuesQueryHandler(IQueueRepository queues)
    {
        _queues = queues ?? throw new ArgumentNullException(nameof(queues));
    }

    public async Task<Result<IReadOnlyList<QueueDto>>> HandleAsync(
        GetAllQueuesQuery query, CancellationToken cancellationToken = default)
    {
        var queues = await _queues.GetAllAsync(cancellationToken);
        return Result.Success(queues.Select(q => q.ToDto()).ToList() as IReadOnlyList<QueueDto>);
    }
}
