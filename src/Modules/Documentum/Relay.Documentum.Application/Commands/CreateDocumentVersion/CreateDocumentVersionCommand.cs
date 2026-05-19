using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.CreateDocumentVersion;

public sealed record CreateDocumentVersionCommand(
    int DocumentId, string DocumentPath, string ContentType, string MimeType,
    long SizeBytes, string CreatedBy, string? Comment = null) : ICommand<UploadDocumentResultDto>;
