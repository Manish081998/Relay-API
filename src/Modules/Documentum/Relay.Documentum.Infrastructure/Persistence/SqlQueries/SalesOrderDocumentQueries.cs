namespace Relay.Documentum.Infrastructure.Persistence.SqlQueries;

internal static class SalesOrderDocumentQueries
{
    // ── Upload new document (inserts master + version 1) ────────────────────
    // Executed as two separate commands inside a transaction (see repository).

    public const string InsertDocument = @"
        INSERT INTO dbo.SalesOrderDocumentDetails
            (OrderSeq, RepPO, BrandName, DocumentName, ContentType, MimeType,
             SizeBytes, CurrentVersion, IsSupportedDocument, IsActive, CreatedBy, CreatedDate)
        VALUES
            (@OrderSeq, @RepPO, @BrandName, @DocumentName, @ContentType, @MimeType,
             @SizeBytes, 1, @IsSupportedDocument, 1, @CreatedBy, GETDATE());
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

    public const string InsertVersion = @"
        INSERT INTO dbo.SalesOrderDocumentVersionDetails
            (DocumentId, VersionNumber, Comment, DocumentPath, ContentType, MimeType,
             SizeBytes, CreatedBy, CreatedDate)
        VALUES
            (@DocumentId, @VersionNumber, @Comment, @DocumentPath, @ContentType, @MimeType,
             @SizeBytes, @CreatedBy, GETDATE());
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

    // ── Create new version (edit/annotate existing document) ────────────────

    public const string GetMaxVersionNumber = @"
        SELECT ISNULL(MAX(VersionNumber), 0)
        FROM dbo.SalesOrderDocumentVersionDetails
        WHERE DocumentId = @DocumentId";

    public const string UpdateCurrentVersion = @"
        UPDATE dbo.SalesOrderDocumentDetails
        SET CurrentVersion = @CurrentVersion,
            ModifiedBy     = @ModifiedBy,
            ModifiedDate   = GETDATE()
        WHERE DocumentId = @DocumentId";

    // ── Reads ───────────────────────────────────────────────────────────────

    public const string GetByOrderSeq = @"
        SELECT DocumentId, OrderSeq, RepPO, BrandName, DocumentName, ContentType,
               MimeType, SizeBytes, CurrentVersion, IsSupportedDocument,
               CreatedBy, CreatedDate, ModifiedBy, ModifiedDate
        FROM dbo.SalesOrderDocumentDetails
        WHERE OrderSeq = @OrderSeq AND IsActive = 1";

    public const string GetByOrderSeqFiltered = @"
        SELECT DocumentId, OrderSeq, RepPO, BrandName, DocumentName, ContentType,
               MimeType, SizeBytes, CurrentVersion, IsSupportedDocument,
               CreatedBy, CreatedDate, ModifiedBy, ModifiedDate
        FROM dbo.SalesOrderDocumentDetails
        WHERE OrderSeq = @OrderSeq AND IsActive = 1
          AND IsSupportedDocument = @IsSupportedDocument";

    public const string GetVersions = @"
        SELECT SalesOrderDocumentVersionId, DocumentId, VersionNumber, Comment,
               DocumentPath, ContentType, MimeType, SizeBytes, CreatedBy, CreatedDate
        FROM dbo.SalesOrderDocumentVersionDetails
        WHERE DocumentId = @DocumentId
        ORDER BY VersionNumber DESC";
}
