using Microsoft.Extensions.Logging;
using Relay.Documentum.Application.Mappers;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.UpdateDocumentById;

public sealed class UpdateDocumentByIdCommandHandler : ICommandHandler<UpdateDocumentByIdCommand, DocumentDto>
{
    private readonly IDocumentRepository _documents;
    private readonly ILogger<UpdateDocumentByIdCommandHandler> _logger;

    public UpdateDocumentByIdCommandHandler(
        IDocumentRepository documents,
        ILogger<UpdateDocumentByIdCommandHandler> logger)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
        _logger    = logger    ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<DocumentDto>> HandleAsync(
        UpdateDocumentByIdCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating document. Id={DocumentId}", command.Id);

        var document = await _documents.GetByIdAsync(command.Id, cancellationToken);

        if (document is null)
        {
            _logger.LogWarning("Document not found. Id={DocumentId}", command.Id);

            return Result.Failure<DocumentDto>(
                new AppError("Document.NotFound", $"Document '{command.Id}' was not found."));
        }

        document.UpdateDetails(command.Title, command.StoragePath, command.SizeInBytes);

        await _documents.UpdateAsync(document, cancellationToken);

        _logger.LogInformation("Document updated successfully. Id={DocumentId}", command.Id);

        return Result.Success(document.ToDto());
    }
}
