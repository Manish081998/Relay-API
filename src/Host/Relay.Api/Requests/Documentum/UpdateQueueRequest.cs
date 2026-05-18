namespace Relay.Api.Requests.Documentum;

public sealed record UpdateQueueRequest(
    int QueueId,
    string QueueName,
    string? Description,
    bool IsActive,
    string ModifiedBy);
