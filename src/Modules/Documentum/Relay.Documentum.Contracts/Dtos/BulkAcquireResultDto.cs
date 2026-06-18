namespace Relay.Documentum.Contracts.Dtos;

public sealed record BulkAcquireResultDto(
    int TotalRequested,
    int AcquiredCount,
    int AlreadyAcquiredCount,
    int NoQueueCount,
    int ErrorCount,
    IReadOnlyList<BulkAcquireItemResultDto> Items);

public sealed record BulkAcquireItemResultDto(
    int OrderSeq,
    string Status,
    string Message);
