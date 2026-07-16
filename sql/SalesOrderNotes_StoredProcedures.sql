-- ============================================================================
-- Sales Order Notes – Stored Procedures
-- Table: dbo.SalesOrderNoteDetails
-- ============================================================================

-- ─── Get Notes by OrderSeq ─────────────────────────────────────────────────

IF OBJECT_ID('dbo.usp_GetSalesOrderNotes', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetSalesOrderNotes;
GO

CREATE PROCEDURE dbo.usp_GetSalesOrderNotes
    @OrderSeq INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        n.SalesOrderNoteId,
        n.OrderSeq,
        n.NotesDescription,
        n.IsActive,
        n.CreatedBy,
        n.CreatedDate,
        n.ModifiedBy,
        n.ModifiedDate,
        ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, ''))), ''), n.CreatedBy) AS CreatedByName
    FROM dbo.SalesOrderNoteDetails n
    LEFT JOIN dbo.UserMaster u ON u.GlobalID = n.CreatedBy
    WHERE n.OrderSeq = @OrderSeq
      AND n.IsActive = 1
    ORDER BY n.CreatedDate DESC;
END;
GO

-- ─── Add a new Note ────────────────────────────────────────────────────────

IF OBJECT_ID('dbo.usp_AddSalesOrderNote', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_AddSalesOrderNote;
GO

CREATE PROCEDURE dbo.usp_AddSalesOrderNote
    @OrderSeq          INT,
    @NotesDescription  NVARCHAR(MAX),
    @CreatedBy         VARCHAR(50),
    @SalesOrderNoteId  BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.SalesOrderNoteDetails
        (OrderSeq, NotesDescription, IsActive, CreatedBy, CreatedDate)
    VALUES
        (@OrderSeq, @NotesDescription, 1, @CreatedBy, GETUTCDATE());

    SET @SalesOrderNoteId = SCOPE_IDENTITY();
END;
GO
