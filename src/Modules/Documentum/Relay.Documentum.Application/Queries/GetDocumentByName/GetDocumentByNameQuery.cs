using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetDocumentByName;

public sealed record GetDocumentByNameQuery(string Name) : IQuery<IReadOnlyList<DocumentDto>>;
