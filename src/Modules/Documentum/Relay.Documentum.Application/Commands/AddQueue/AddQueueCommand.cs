using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.AddQueue;

public sealed record AddQueueCommand(
    string QueueName,
    string? Description,
    bool IsActive,
    string CreatedBy) : ICommand<int>;
