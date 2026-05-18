namespace Relay.Documentum.Domain.Aggregates;

public sealed record Queue(
    int QueueId,
    string QueueName,
    string? Description,
    bool IsActive,
    string CreatedBy,
    DateTime CreatedDate,
    string? ModifiedBy,
    DateTime? ModifiedDate);
