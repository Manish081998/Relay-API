using Relay.Documentum.Domain.Aggregates;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.AddQueue;

public sealed class AddQueueCommandHandler : ICommandHandler<AddQueueCommand, int>
{
    private readonly IQueueRepository _queues;

    public AddQueueCommandHandler(IQueueRepository queues)
    {
        _queues = queues ?? throw new ArgumentNullException(nameof(queues));
    }

    public async Task<Result<int>> HandleAsync(AddQueueCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var queue = new Queue(
            QueueId:      0,
            QueueName:    command.QueueName,
            Description:  command.Description,
            IsActive:     command.IsActive,
            CreatedBy:    command.CreatedBy,
            CreatedDate:  now,
            ModifiedBy:   command.CreatedBy,
            ModifiedDate: now);

        var newId = await _queues.AddAsync(queue, cancellationToken);

        return Result.Success(newId);
    }
}
