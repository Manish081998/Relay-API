using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.UploadSalesOrderDocument;

public sealed record UploadSalesOrderDocumentCommand(
    int OrderSeq, string? RepPO, string? BrandName, string DocumentName,
    string ContentType, string MimeType, long SizeBytes, string DocumentPath,
    bool IsSupportedDocument, string CreatedBy) : ICommand<UploadDocumentResultDto>;
