namespace Relay.Documentum.Contracts.Dtos;

public sealed record SalesOrderDocumentDto(
    int DocumentId, int OrderSeq, string? RepPO, string? BrandName,
    string DocumentName, string ContentType, string MimeType, long SizeBytes,
    int CurrentVersion, bool IsSupportedDocument, string CreatedBy, DateTime CreatedDate,
    string? ModifiedBy, DateTime? ModifiedDate, string? CreatedByName);

public sealed record SalesOrderDocumentVersionDto(
    int SalesOrderDocumentVersionId, int DocumentId, int VersionNumber,
    string? Comment, string DocumentPath, string ContentType, string MimeType,
    long SizeBytes, string CreatedBy, DateTime CreatedDate, string? CreatedByName);

public sealed record UploadDocumentResultDto(
    int DocumentId, int VersionId, int VersionNumber, string DocumentPath);
