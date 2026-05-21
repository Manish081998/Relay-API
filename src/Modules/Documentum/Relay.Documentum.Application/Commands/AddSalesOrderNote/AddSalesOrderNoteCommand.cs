using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.AddSalesOrderNote;

public sealed record AddSalesOrderNoteCommand(
    int OrderSeq,
    string NotesDescription,
    string CreatedBy) : ICommand<long>;
