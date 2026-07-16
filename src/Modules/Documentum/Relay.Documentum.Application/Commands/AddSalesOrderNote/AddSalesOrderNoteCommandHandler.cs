using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.AddSalesOrderNote;

public sealed class AddSalesOrderNoteCommandHandler : ICommandHandler<AddSalesOrderNoteCommand, long>
{
    private readonly ISalesOrderNoteRepository _notes;

    public AddSalesOrderNoteCommandHandler(ISalesOrderNoteRepository notes)
    {
        _notes = notes ?? throw new ArgumentNullException(nameof(notes));
    }

    public async Task<Result<long>> HandleAsync(
        AddSalesOrderNoteCommand command, CancellationToken cancellationToken = default)
    {
        var noteId = await _notes.AddAsync(
            command.OrderSeq, command.NotesDescription, command.CreatedBy, cancellationToken);

        return Result.Success(noteId);
    }
}
