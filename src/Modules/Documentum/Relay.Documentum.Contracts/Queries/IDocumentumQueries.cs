using Relay.Documentum.Contracts.Dtos;

namespace Relay.Documentum.Contracts.Queries;

public interface IDocumentumQueries
{
    Task<DocumentDto?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}
