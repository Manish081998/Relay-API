namespace Relay.WebTool.Contracts.Dtos;

public sealed record SelectionOptionDto(Guid Id, string Label, string Value, int DisplayOrder);

public sealed record SelectionDto(
    Guid Id,
    string Title,
    Guid SubmittedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SubmittedAt,
    IReadOnlyList<SelectionOptionDto> Options);
