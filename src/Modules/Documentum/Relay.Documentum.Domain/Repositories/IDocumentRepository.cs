using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Domain.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
}
