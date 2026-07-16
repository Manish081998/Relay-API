using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetSalesOrderNotes;

public sealed record GetSalesOrderNotesQuery(int OrderSeq) : IQuery<IReadOnlyList<SalesOrderNoteDto>>;
