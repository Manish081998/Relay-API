using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.DeleteQueue;

public sealed record DeleteQueueCommand(int QueueId) : ICommand<int>;
