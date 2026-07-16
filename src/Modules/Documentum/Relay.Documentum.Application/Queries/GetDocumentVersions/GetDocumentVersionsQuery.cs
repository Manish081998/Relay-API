using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetDocumentVersions;

public sealed record GetDocumentVersionsQuery(int DocumentId) : IQuery<IReadOnlyList<SalesOrderDocumentVersionDto>>;
