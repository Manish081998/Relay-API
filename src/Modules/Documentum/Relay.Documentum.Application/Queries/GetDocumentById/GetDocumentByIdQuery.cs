using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetDocumentById;

public sealed record GetDocumentByIdQuery(Guid DocumentId) : IQuery<DocumentDto?>;