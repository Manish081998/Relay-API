namespace Relay.Documentum.Contracts.Dtos;

public sealed record QueueDto(
    int QueueId,
    string QueueName,
    string? Description,
    bool IsActive,
    string CreatedBy,
    DateTime CreatedDate);
