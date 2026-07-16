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
IF OBJECT_ID('dbo.usp_GetSalesOrderDocuments', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetSalesOrderDocuments;
GO

CREATE PROCEDURE [dbo].[usp_GetSalesOrderDocuments]
    @OrderSeq            INT,
    @IsSupportedDocument BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.DocumentId,
        d.OrderSeq,
        d.RepPO,
        d.BrandName,
        d.DocumentName,
        d.ContentType,
        d.MimeType,
        d.SizeBytes,
        d.CurrentVersion,
        d.IsSupportedDocument,
        d.IsActive,
        d.CreatedBy,
        d.CreatedDate,
        d.ModifiedBy,
        d.ModifiedDate,
        ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, ''))), ''), d.CreatedBy) AS CreatedByName
    FROM [dbo].[SalesOrderDocumentDetails] d
    LEFT JOIN [dbo].[UserMaster] u ON u.GlobalID = d.CreatedBy
    WHERE d.OrderSeq = @OrderSeq
      AND d.IsActive = 1
      AND (@IsSupportedDocument IS NULL OR d.IsSupportedDocument = @IsSupportedDocument)
    ORDER BY d.CreatedDate DESC;
END;
GO

-- 4. Get all versions for a document
IF OBJECT_ID('dbo.usp_GetSalesOrderDocVersions', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetSalesOrderDocVersions;
GO

CREATE PROCEDURE [dbo].[usp_GetSalesOrderDocVersions]
    @DocumentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.SalesOrderDocumentVersionId,
        v.DocumentId,
        v.VersionNumber,
        v.Comment,
        v.DocumentPath,
        v.ContentType,
        v.MimeType,
        v.SizeBytes,
        v.CreatedBy,
        v.CreatedDate,
        ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, ''))), ''), v.CreatedBy) AS CreatedByName
    FROM [dbo].[SalesOrderDocumentVersionDetails] v
    LEFT JOIN [dbo].[UserMaster] u ON u.GlobalID = v.CreatedBy
    WHERE v.DocumentId = @DocumentId
    ORDER BY v.VersionNumber DESC;
END;
GO
