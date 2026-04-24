using Relay.Documentum.Application.Mappers;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetDocumentByName;

public sealed class GetDocumentByNameQueryHandler : IQueryHandler<GetDocumentByNameQuery, IReadOnlyList<DocumentDto>>
{
    private readonly IDocumentRepository _documents;

    public GetDocumentByNameQueryHandler(IDocumentRepository documents)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
    }

    public async Task<Result<IReadOnlyList<DocumentDto>>> HandleAsync(
        GetDocumentByNameQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Name))
            return Result.Failure<IReadOnlyList<DocumentDto>>(
                new AppError("Document.NameRequired", "Search name must not be empty."));

        var documents = await _documents.GetByNameAsync(query.Name, cancellationToken);
        var dtos = documents.Select(d => d.ToDto()).ToList();
        return Result.Success<IReadOnlyList<DocumentDto>>(dtos);
    }
}
