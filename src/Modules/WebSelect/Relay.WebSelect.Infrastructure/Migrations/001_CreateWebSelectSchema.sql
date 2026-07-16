-- WebTool module baseline schema.

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'WebTool')
    EXEC('CREATE SCHEMA WebTool');
GO

IF OBJECT_ID('WebTool.Selections', 'U') IS NULL
BEGIN
    CREATE TABLE WebTool.Selections
    (
        Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Title        NVARCHAR(200)    NOT NULL,
        SubmittedBy  UNIQUEIDENTIFIER NOT NULL,
        CreatedAt    DATETIMEOFFSET   NOT NULL,
        SubmittedAt  DATETIMEOFFSET   NULL
    );

    CREATE INDEX IX_WebTool_Selections_SubmittedBy ON WebTool.Selections (SubmittedBy);
END
GO

IF OBJECT_ID('WebTool.SelectionOptions', 'U') IS NULL
BEGIN
    CREATE TABLE WebTool.SelectionOptions
    (
        Id            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        SelectionId   UNIQUEIDENTIFIER NOT NULL,
        Label         NVARCHAR(200)    NOT NULL,
        Value         NVARCHAR(500)    NOT NULL,
        DisplayOrder  INT              NOT NULL,
        CONSTRAINT FK_WebTool_SelectionOptions_Selections
            FOREIGN KEY (SelectionId) REFERENCES WebTool.Selections (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_WebTool_SelectionOptions_SelectionId
        ON WebTool.SelectionOptions (SelectionId);
END
GO
