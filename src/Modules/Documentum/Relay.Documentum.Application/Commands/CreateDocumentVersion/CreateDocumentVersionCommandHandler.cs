using Microsoft.Extensions.Logging;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.CreateDocumentVersion;

public sealed class CreateDocumentVersionCommandHandler
    : ICommandHandler<CreateDocumentVersionCommand, UploadDocumentResultDto>
{
    private readonly ISalesOrderDocumentRepository _docs;
    private readonly ILogger<CreateDocumentVersionCommandHandler> _logger;

    public CreateDocumentVersionCommandHandler(
        ISalesOrderDocumentRepository docs,
        ILogger<CreateDocumentVersionCommandHandler> logger)
    {
        _docs   = docs   ?? throw new ArgumentNullException(nameof(docs));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<UploadDocumentResultDto>> HandleAsync(
        CreateDocumentVersionCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new version for DocumentId={DocumentId}", command.DocumentId);

        var (docId, verId, verNum) = await _docs.CreateVersionAsync(
            command.DocumentId, command.DocumentPath, command.ContentType, command.MimeType,
            command.SizeBytes, command.CreatedBy, command.Comment, cancellationToken);

        _logger.LogInformation("Version created. DocumentId={DocumentId}, Version={VersionNumber}",
            docId, verNum);

        return Result.Success(new UploadDocumentResultDto(docId, verId, verNum, command.DocumentPath));
    }
}
