using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.UpdateQueue;

public sealed record UpdateQueueCommand(
    int QueueId,
    string QueueName,
    string? Description,
    bool IsActive,
    string ModifiedBy) : ICommand<int>;
