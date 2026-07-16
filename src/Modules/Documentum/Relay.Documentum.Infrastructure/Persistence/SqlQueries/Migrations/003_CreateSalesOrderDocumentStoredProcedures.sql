-- ============================================================================
-- Migration: Create stored procedures for SalesOrderDocument operations
--   1. usp_UploadSalesOrderDocument     — Insert master + version 1 in one call
--   2. usp_CreateSalesOrderDocVersion   — Add new version, auto-increment, update master
--   3. usp_GetSalesOrderDocuments       — Get documents by OrderSeq with optional filter
--   4. usp_GetSalesOrderDocVersions     — Get version history for a document
-- Date: 2026-05-20
-- ============================================================================

-- ────────────────────────────────────────────────────────────────────────────
-- 1. Upload new document (master + version 1) — atomic, no app-side transaction needed
-- ────────────────────────────────────────────────────────────────────────────
CREATE OR ALTER PROCEDURE [dbo].[usp_UploadSalesOrderDocument]
    @OrderSeq             INT,
    @RepPO                VARCHAR(50)  = NULL,
    @BrandName            VARCHAR(50)  = NULL,
    @DocumentName         VARCHAR(255),
    @ContentType          VARCHAR(50),
    @MimeType             VARCHAR(100),
    @SizeBytes            BIGINT,
    @DocumentPath         VARCHAR(500),
    @IsSupportedDocument  BIT,
    @CreatedBy            VARCHAR(100),
    @DocumentId           INT          OUTPUT,
    @VersionId            INT          OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    -- Insert document master
    INSERT INTO dbo.SalesOrderDocumentDetails
        (OrderSeq, RepPO, BrandName, DocumentName, ContentType, MimeType,
         SizeBytes, CurrentVersion, IsSupportedDocument, IsActive, CreatedBy, CreatedDate)
    VALUES
        (@OrderSeq, @RepPO, @BrandName, @DocumentName, @ContentType, @MimeType,
         @SizeBytes, 1, @IsSupportedDocument, 1, @CreatedBy, GETDATE());

    SET @DocumentId = SCOPE_IDENTITY();

    -- Insert version 1
    INSERT INTO dbo.SalesOrderDocumentVersionDetails
        (DocumentId, VersionNumber, Comment, DocumentPath, ContentType, MimeType,
         SizeBytes, CreatedBy, CreatedDate)
    VALUES
        (@DocumentId, 1, NULL, @DocumentPath, @ContentType, @MimeType,
         @SizeBytes, @CreatedBy, GETDATE());

    SET @VersionId = SCOPE_IDENTITY();

    COMMIT TRANSACTION;

    -- Return the IDs and version number
    SELECT @DocumentId AS DocumentId, @VersionId AS VersionId, 1 AS VersionNumber;
END
GO

-- ────────────────────────────────────────────────────────────────────────────
-- 2. Create new version — auto-increments, updates master, fully atomic
-- ────────────────────────────────────────────────────────────────────────────
CREATE OR ALTER PROCEDURE [dbo].[usp_CreateSalesOrderDocVersion]
    @DocumentId    INT,
    @DocumentPath  VARCHAR(500),
    @ContentType   VARCHAR(50),
    @MimeType      VARCHAR(100),
    @SizeBytes     BIGINT,
    @CreatedBy     VARCHAR(100),
    @Comment       VARCHAR(500)  = NULL,
    @VersionId     INT           OUTPUT,
    @VersionNumber INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    -- Determine next version number
    SELECT @VersionNumber = ISNULL(MAX(VersionNumber), 0) + 1
    FROM dbo.SalesOrderDocumentVersionDetails WITH (UPDLOCK, HOLDLOCK)
    WHERE DocumentId = @DocumentId;

    -- Insert new version
    INSERT INTO dbo.SalesOrderDocumentVersionDetails
        (DocumentId, VersionNumber, Comment, DocumentPath, ContentType, MimeType,
         SizeBytes, CreatedBy, CreatedDate)
    VALUES
        (@DocumentId, @VersionNumber, @Comment, @DocumentPath, @ContentType, @MimeType,
         @SizeBytes, @CreatedBy, GETDATE());

    SET @VersionId = SCOPE_IDENTITY();

    -- Update master record
    UPDATE dbo.SalesOrderDocumentDetails
    SET CurrentVersion = @VersionNumber,
        ModifiedBy     = @CreatedBy,
        ModifiedDate   = GETDATE()
    WHERE DocumentId = @DocumentId;

    COMMIT TRANSACTION;

    -- Return result
    SELECT @DocumentId AS DocumentId, @VersionId AS VersionId, @VersionNumber AS VersionNumber;
END
GO

-- ────────────────────────────────────────────────────────────────────────────
-- 3. Get documents by OrderSeq — optional IsSupportedDocument filter
-- ────────────────────────────────────────────────────────────────────────────
CREATE OR ALTER PROCEDURE [dbo].[usp_GetSalesOrderDocuments]
    @OrderSeq             INT,
    @IsSupportedDocument  BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DocumentId, OrderSeq, RepPO, BrandName, DocumentName, ContentType,
           MimeType, SizeBytes, CurrentVersion, IsSupportedDocument,
           CreatedBy, CreatedDate, ModifiedBy, ModifiedDate
    FROM dbo.SalesOrderDocumentDetails WITH (NOLOCK)
    WHERE OrderSeq = @OrderSeq
      AND IsActive = 1
      AND (@IsSupportedDocument IS NULL OR IsSupportedDocument = @IsSupportedDocument)
    ORDER BY CreatedDate DESC;
END
GO

-- ────────────────────────────────────────────────────────────────────────────
-- 4. Get version history for a document — descending by version number
-- ────────────────────────────────────────────────────────────────────────────
CREATE OR ALTER PROCEDURE [dbo].[usp_GetSalesOrderDocVersions]
    @DocumentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT SalesOrderDocumentVersionId, DocumentId, VersionNumber, Comment,
           DocumentPath, ContentType, MimeType, SizeBytes, CreatedBy, CreatedDate
    FROM dbo.SalesOrderDocumentVersionDetails WITH (NOLOCK)
    WHERE DocumentId = @DocumentId
    ORDER BY VersionNumber DESC;
END
GO
