using Relay.Documentum.Application.Mappers;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetDocumentById;

public sealed class GetDocumentByIdQueryHandler : IQueryHandler<GetDocumentByIdQuery, DocumentDto?>
{
    private readonly IDocumentRepository _documents;

    public GetDocumentByIdQueryHandler(IDocumentRepository documents)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
    }

    public async Task<Result<DocumentDto?>> HandleAsync(GetDocumentByIdQuery query, CancellationToken cancellationToken = default)
    {
        var document = await _documents.GetByIdAsync(query.DocumentId, cancellationToken);
        return Result.Success(document?.ToDto());
    }
}