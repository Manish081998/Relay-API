namespace Relay.Documentum.Domain.Repositories;

public interface ISalesOrderDocumentRepository
{
    Task<(int DocumentId, int VersionId, int VersionNumber)> UploadAsync(
        int orderSeq, string? repPO, string? brandName, string documentName,
        string contentType, string mimeType, long sizeBytes, string documentPath,
        bool isSupportedDocument, string createdBy, CancellationToken cancellationToken = default);

    Task<(int DocumentId, int VersionId, int VersionNumber)> CreateVersionAsync(
        int documentId, string documentPath, string contentType, string mimeType,
        long sizeBytes, string createdBy, string? comment = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SalesOrderDocumentResult>> GetByOrderSeqAsync(
        int orderSeq, bool? isSupportedDocument = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SalesOrderDocumentVersionResult>> GetVersionsAsync(
        int documentId, CancellationToken cancellationToken = default);
}

public sealed record SalesOrderDocumentResult(
    int DocumentId, int OrderSeq, string? RepPO, string? BrandName,
    string DocumentName, string ContentType, string MimeType, long SizeBytes,
    int CurrentVersion, bool IsSupportedDocument, string CreatedBy, DateTime CreatedDate,
    string? ModifiedBy, DateTime? ModifiedDate, string? CreatedByName);

public sealed record SalesOrderDocumentVersionResult(
    int SalesOrderDocumentVersionId, int DocumentId, int VersionNumber,
    string? Comment, string DocumentPath, string ContentType, string MimeType,
    long SizeBytes, string CreatedBy, DateTime CreatedDate, string? CreatedByName);
