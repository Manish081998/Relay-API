namespace Relay.Documentum.Contracts.Dtos;

public sealed record WorkflowHistoryItemDto(
    string ActivityName,
    string Comments,
    string UserName,
    DateTime Timestamp,
    string EventType,
    string? OrderStatus);
