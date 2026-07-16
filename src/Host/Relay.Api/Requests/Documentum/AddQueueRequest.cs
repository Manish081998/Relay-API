namespace Relay.Api.Requests.Documentum;

public sealed record AddQueueRequest(
    string QueueName,
    string? Description,
    bool IsActive);
