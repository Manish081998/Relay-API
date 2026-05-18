using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.DeleteQueue;

public sealed class DeleteQueueCommandHandler : ICommandHandler<DeleteQueueCommand, int>
{
    private readonly IQueueRepository _queues;

    public DeleteQueueCommandHandler(IQueueRepository queues)
    {
        _queues = queues ?? throw new ArgumentNullException(nameof(queues));
    }

    public async Task<Result<int>> HandleAsync(DeleteQueueCommand command, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _queues.DeleteAsync(command.QueueId, cancellationToken);

        if (rowsAffected == 0)
            return Result.Failure<int>(new AppError("Queue.NotFound", $"Queue '{command.QueueId}' was not found."));

        return Result.Success(command.QueueId);
    }
}
