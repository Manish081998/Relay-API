namespace Relay.Documentum.Domain.Repositories;

public interface IWorkflowRepository
{
    Task<WorkflowActionResult> ProcessActionAsync(
        int orderSeq, int actionFlag, string userGlobalId,
        int? destinationQueueId, string comment,
        CancellationToken cancellationToken = default);

    Task<WorkflowStateResult?> GetStateAsync(
        int orderSeq, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkflowHistoryResult>> GetHistoryAsync(
        int orderSeq, CancellationToken cancellationToken = default);

    Task<string?> GetQueueNameAsync(
        int queueId, CancellationToken cancellationToken = default);

    Task<string> GetUserDisplayNameAsync(
        string globalId, CancellationToken cancellationToken = default);
}

public sealed record WorkflowActionResult(int StatusCode, string StatusMessage);

public sealed record WorkflowStateResult(
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

public sealed record WorkflowHistoryResult(
    string ActivityName,
    string Comments,
    string UserName,
    DateTime Timestamp,
    string EventType,
    string? OrderStatus);
