using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Contracts.Queries;
using Relay.Documentum.Domain.Aggregates;
using Relay.Documentum.Domain.Repositories;

namespace Relay.Documentum.Infrastructure;

internal sealed class DocumentumQueriesAdapter : IDocumentumQueries
{
    private readonly IDocumentRepository _documents;

    public DocumentumQueriesAdapter(IDocumentRepository documents)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documents.GetByIdAsync(documentId, cancellationToken);
        return document is null ? null : ToDto(document);
    }

    private static DocumentDto ToDto(Document document) => new(
        document.Id, document.Title, document.Status.Name,
        document.OwnerId, document.SizeInBytes, document.CreatedAt, document.PublishedAt);
}
