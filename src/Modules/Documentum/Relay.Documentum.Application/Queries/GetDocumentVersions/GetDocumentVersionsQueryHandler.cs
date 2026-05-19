using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetDocumentVersions;

public sealed class GetDocumentVersionsQueryHandler
    : IQueryHandler<GetDocumentVersionsQuery, IReadOnlyList<SalesOrderDocumentVersionDto>>
{
    private readonly ISalesOrderDocumentRepository _docs;

    public GetDocumentVersionsQueryHandler(ISalesOrderDocumentRepository docs)
    {
        _docs = docs ?? throw new ArgumentNullException(nameof(docs));
    }

    public async Task<Result<IReadOnlyList<SalesOrderDocumentVersionDto>>> HandleAsync(
        GetDocumentVersionsQuery query, CancellationToken cancellationToken = default)
    {
        var items = await _docs.GetVersionsAsync(query.DocumentId, cancellationToken);

        var dtos = items.Select(v => new SalesOrderDocumentVersionDto(
            v.SalesOrderDocumentVersionId, v.DocumentId, v.VersionNumber,
            v.Comment, v.DocumentPath, v.ContentType, v.MimeType,
            v.SizeBytes, v.CreatedBy, v.CreatedDate)).ToList();

        return Result.Success<IReadOnlyList<SalesOrderDocumentVersionDto>>(dtos);
    }
}
