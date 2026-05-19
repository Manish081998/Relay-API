-- =============================================
-- Sales Order Document Stored Procedures
-- =============================================

-- 1. Upload a new Sales Order Document (inserts master + version 1)
IF OBJECT_ID('dbo.usp_UploadSalesOrderDocument', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_UploadSalesOrderDocument;
GO

CREATE PROCEDURE [dbo].[usp_UploadSalesOrderDocument]
    @OrderSeq           INT,
    @RepPO              NVARCHAR(255)   = NULL,
    @BrandName          NVARCHAR(255)   = NULL,
    @DocumentName       NVARCHAR(500),
    @ContentType        NVARCHAR(100),
    @MimeType           NVARCHAR(255),
    @SizeBytes          BIGINT,
    @DocumentPath       NVARCHAR(1000),
    @IsSupportedDocument BIT,
    @CreatedBy          NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DocumentId INT;
    DECLARE @VersionId  INT;
    DECLARE @Now        DATETIME = GETUTCDATE();

    -- Insert into master table
    INSERT INTO [dbo].[SalesOrderDocumentDetails]
    (
        OrderSeq, RepPO, BrandName, DocumentName, ContentType, MimeType,
        SizeBytes, CurrentVersion, IsSupportedDocument, IsActive,
        CreatedBy, CreatedDate
    )
    VALUES
    (
        @OrderSeq, @RepPO, @BrandName, @DocumentName, @ContentType, @MimeType,
        @SizeBytes, 1, @IsSupportedDocument, 1,
        @CreatedBy, @Now
    );

    SET @DocumentId = SCOPE_IDENTITY();

    -- Insert version 1
    INSERT INTO [dbo].[SalesOrderDocumentVersionDetails]
    (
        DocumentId, VersionNumber, Comment, DocumentPath,
        ContentType, MimeType, SizeBytes, CreatedBy, CreatedDate
    )
    VALUES
    (
        @DocumentId, 1, NULL, @DocumentPath,
        @ContentType, @MimeType, @SizeBytes, @CreatedBy, @Now
    );

    SET @VersionId = SCOPE_IDENTITY();

    -- Return result
    SELECT
        @DocumentId     AS DocumentId,
        @VersionId      AS SalesOrderDocumentVersionId,
        1               AS VersionNumber,
        @DocumentPath   AS DocumentPath;
END;
GO

-- 2. Create a new version for an existing document
IF OBJECT_ID('dbo.usp_CreateDocumentVersion', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_CreateDocumentVersion;
GO

CREATE PROCEDURE [dbo].[usp_CreateDocumentVersion]
    @DocumentId     INT,
    @DocumentPath   NVARCHAR(1000),
    @ContentType    NVARCHAR(100),
    @MimeType       NVARCHAR(255),
    @SizeBytes      BIGINT,
    @CreatedBy      NVARCHAR(255),
    @Comment        NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NextVersion INT;
    DECLARE @VersionId   INT;
    DECLARE @Now         DATETIME = GETUTCDATE();

    -- Determine next version number
    SELECT @NextVersion = ISNULL(MAX(VersionNumber), 0) + 1
    FROM [dbo].[SalesOrderDocumentVersionDetails]
    WHERE DocumentId = @DocumentId;

    -- Insert new version
    INSERT INTO [dbo].[SalesOrderDocumentVersionDetails]
    (
        DocumentId, VersionNumber, Comment, DocumentPath,
        ContentType, MimeType, SizeBytes, CreatedBy, CreatedDate
    )
    VALUES
    (
        @DocumentId, @NextVersion, @Comment, @DocumentPath,
        @ContentType, @MimeType, @SizeBytes, @CreatedBy, @Now
    );

    SET @VersionId = SCOPE_IDENTITY();

    -- Update current version on master table
    UPDATE [dbo].[SalesOrderDocumentDetails]
    SET CurrentVersion = @NextVersion,
        ModifiedBy     = @CreatedBy,
        ModifiedDate   = @Now
    WHERE DocumentId = @DocumentId;

    -- Return result
    SELECT
        @DocumentId     AS DocumentId,
        @VersionId      AS SalesOrderDocumentVersionId,
        @NextVersion    AS VersionNumber,
        @DocumentPath   AS DocumentPath;
END;
GO

-- 3. Get documents by OrderSeq (with optional IsSupportedDocument filter)
IF OBJECT_ID('dbo.usp_GetDocumentsByOrderSeq', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetDocumentsByOrderSeq;
GO

CREATE PROCEDURE [dbo].[usp_GetDocumentsByOrderSeq]
    @OrderSeq            INT,
    @IsSupportedDocument BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DocumentId,
        OrderSeq,
        RepPO,
        BrandName,
        DocumentName,
        ContentType,
        MimeType,
        SizeBytes,
        CurrentVersion,
        IsSupportedDocument,
        IsActive,
        CreatedBy,
        CreatedDate,
        ModifiedBy,
        ModifiedDate
    FROM [dbo].[SalesOrderDocumentDetails]
    WHERE OrderSeq = @OrderSeq
      AND IsActive = 1
      AND (@IsSupportedDocument IS NULL OR IsSupportedDocument = @IsSupportedDocument)
    ORDER BY CreatedDate DESC;
END;
GO

-- 4. Get all versions for a document
IF OBJECT_ID('dbo.usp_GetDocumentVersions', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetDocumentVersions;
GO

CREATE PROCEDURE [dbo].[usp_GetDocumentVersions]
    @DocumentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SalesOrderDocumentVersionId,
        DocumentId,
        VersionNumber,
        Comment,
        DocumentPath,
        ContentType,
        MimeType,
        SizeBytes,
        CreatedBy,
        CreatedDate
    FROM [dbo].[SalesOrderDocumentVersionDetails]
    WHERE DocumentId = @DocumentId
    ORDER BY VersionNumber DESC;
END;
GO
