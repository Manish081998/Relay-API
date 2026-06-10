namespace Relay.Documentum.Contracts.Dtos;

public sealed record WorkflowStateDto(
    long EdgeOrderStateId,
    int OrderSeq,
    int CurrentQueueId,
    string QueueName,
    bool IsAcquired,
    string? AcquiredBy,
    string? AcquiredByName,
    DateTime? StageChangeDate,
    DateTime? CompletionDate,
    DateTime CreatedDate,
    DateTime StartedOn);
