using Microsoft.Extensions.Logging;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.UploadSalesOrderDocument;

public sealed class UploadSalesOrderDocumentCommandHandler
    : ICommandHandler<UploadSalesOrderDocumentCommand, UploadDocumentResultDto>
{
    private readonly ISalesOrderDocumentRepository _docs;
    private readonly ILogger<UploadSalesOrderDocumentCommandHandler> _logger;

    public UploadSalesOrderDocumentCommandHandler(
        ISalesOrderDocumentRepository docs,
        ILogger<UploadSalesOrderDocumentCommandHandler> logger)
    {
        _docs   = docs   ?? throw new ArgumentNullException(nameof(docs));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<UploadDocumentResultDto>> HandleAsync(
        UploadSalesOrderDocumentCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading document {DocumentName} for OrderSeq={OrderSeq}",
            command.DocumentName, command.OrderSeq);

        var (docId, verId, verNum) = await _docs.UploadAsync(
            command.OrderSeq, command.RepPO, command.BrandName, command.DocumentName,
            command.ContentType, command.MimeType, command.SizeBytes, command.DocumentPath,
            command.IsSupportedDocument, command.CreatedBy, cancellationToken);

        _logger.LogInformation("Document uploaded. DocumentId={DocumentId}, VersionId={VersionId}",
            docId, verId);

        return Result.Success(new UploadDocumentResultDto(docId, verId, verNum, command.DocumentPath));
    }
}
