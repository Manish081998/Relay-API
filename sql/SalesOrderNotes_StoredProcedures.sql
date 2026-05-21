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
        SalesOrderNoteId,
        OrderSeq,
        NotesDescription,
        IsActive,
        CreatedBy,
        CreatedDate,
        ModifiedBy,
        ModifiedDate
    FROM dbo.SalesOrderNoteDetails
    WHERE OrderSeq = @OrderSeq
      AND IsActive = 1
    ORDER BY CreatedDate DESC;
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
