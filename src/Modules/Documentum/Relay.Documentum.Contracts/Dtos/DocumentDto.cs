namespace Relay.Documentum.Contracts.Dtos;

public sealed record DocumentDto(
    Guid Id,
    string Title,
    string Status,
    Guid OwnerId,
    long SizeInBytes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt);
