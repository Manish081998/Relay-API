using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetDocumentsByOrderSeq;

public sealed class GetDocumentsByOrderSeqQueryHandler
    : IQueryHandler<GetDocumentsByOrderSeqQuery, IReadOnlyList<SalesOrderDocumentDto>>
{
    private readonly ISalesOrderDocumentRepository _docs;

    public GetDocumentsByOrderSeqQueryHandler(ISalesOrderDocumentRepository docs)
    {
        _docs = docs ?? throw new ArgumentNullException(nameof(docs));
    }

    public async Task<Result<IReadOnlyList<SalesOrderDocumentDto>>> HandleAsync(
        GetDocumentsByOrderSeqQuery query, CancellationToken cancellationToken = default)
    {
        var items = await _docs.GetByOrderSeqAsync(query.OrderSeq, query.IsSupportedDocument, cancellationToken);

        var dtos = items.Select(d => new SalesOrderDocumentDto(
            d.DocumentId, d.OrderSeq, d.RepPO, d.BrandName, d.DocumentName,
            d.ContentType, d.MimeType, d.SizeBytes, d.CurrentVersion,
            d.IsSupportedDocument, d.CreatedBy, d.CreatedDate,
            d.ModifiedBy, d.ModifiedDate, d.CreatedByName)).ToList();

        return Result.Success<IReadOnlyList<SalesOrderDocumentDto>>(dtos);
    }
}
