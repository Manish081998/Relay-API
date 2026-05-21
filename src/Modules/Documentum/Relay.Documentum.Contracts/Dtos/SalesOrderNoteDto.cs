namespace Relay.Documentum.Contracts.Dtos;

public sealed record SalesOrderNoteDto(
    long SalesOrderNoteId,
    int OrderSeq,
    string NotesDescription,
    bool IsActive,
    string CreatedBy,
    DateTime CreatedDate,
    string? ModifiedBy,
    DateTime? ModifiedDate);
