namespace Relay.Api.Requests.Documentum;

public sealed record AddSalesOrderNoteRequest(
    int OrderSeq,
    string NotesDescription);
