namespace Relay.Documentum.Domain.Repositories;

public interface ISalesOrderNoteRepository
{
    Task<long> AddAsync(
        int orderSeq, string notesDescription, string createdBy,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SalesOrderNoteResult>> GetByOrderSeqAsync(
        int orderSeq, CancellationToken cancellationToken = default);
}

public sealed record SalesOrderNoteResult(
    long SalesOrderNoteId,
    int OrderSeq,
    string NotesDescription,
    bool IsActive,
    string CreatedBy,
    DateTime CreatedDate,
    string? ModifiedBy,
    DateTime? ModifiedDate);
