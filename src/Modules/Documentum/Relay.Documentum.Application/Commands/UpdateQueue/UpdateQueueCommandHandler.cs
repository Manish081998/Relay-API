using Relay.Documentum.Domain.Aggregates;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.UpdateQueue;

public sealed class UpdateQueueCommandHandler : ICommandHandler<UpdateQueueCommand, int>
{
    private readonly IQueueRepository _queues;

    public UpdateQueueCommandHandler(IQueueRepository queues)
    {
        _queues = queues ?? throw new ArgumentNullException(nameof(queues));
    }

    public async Task<Result<int>> HandleAsync(UpdateQueueCommand command, CancellationToken cancellationToken = default)
    {
        var queue = new Queue(
            QueueId:      command.QueueId,
            QueueName:    command.QueueName,
            Description:  command.Description,
            IsActive:     command.IsActive,
            CreatedBy:    string.Empty,
            CreatedDate:  default,
            ModifiedBy:   command.ModifiedBy,
            ModifiedDate: DateTime.UtcNow);

        var rowsAffected = await _queues.UpdateAsync(queue, cancellationToken);

        if (rowsAffected == 0)
            return Result.Failure<int>(new AppError("Queue.NotFound", $"Queue '{command.QueueId}' was not found."));

        return Result.Success(command.QueueId);
    }
}
