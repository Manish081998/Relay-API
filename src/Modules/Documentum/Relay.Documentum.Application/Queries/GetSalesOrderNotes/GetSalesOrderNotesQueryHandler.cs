using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetSalesOrderNotes;

public sealed class GetSalesOrderNotesQueryHandler
    : IQueryHandler<GetSalesOrderNotesQuery, IReadOnlyList<SalesOrderNoteDto>>
{
    private readonly ISalesOrderNoteRepository _notes;

    public GetSalesOrderNotesQueryHandler(ISalesOrderNoteRepository notes)
    {
        _notes = notes ?? throw new ArgumentNullException(nameof(notes));
    }

    public async Task<Result<IReadOnlyList<SalesOrderNoteDto>>> HandleAsync(
        GetSalesOrderNotesQuery query, CancellationToken cancellationToken = default)
    {
        var items = await _notes.GetByOrderSeqAsync(query.OrderSeq, cancellationToken);

        var dtos = items.Select(n => new SalesOrderNoteDto(
            n.SalesOrderNoteId, n.OrderSeq, n.NotesDescription, n.IsActive,
            n.CreatedBy, n.CreatedDate, n.ModifiedBy, n.ModifiedDate)).ToList();

        return Result.Success<IReadOnlyList<SalesOrderNoteDto>>(dtos);
    }
}
