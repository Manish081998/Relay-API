-- Documentum module baseline schema.

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'documentum')
    EXEC('CREATE SCHEMA documentum');
GO

IF OBJECT_ID('documentum.Documents', 'U') IS NULL
BEGIN
    CREATE TABLE documentum.Documents
    (
        Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Title        NVARCHAR(200)    NOT NULL,
        StoragePath  NVARCHAR(1024)   NOT NULL,
        OwnerId      UNIQUEIDENTIFIER NOT NULL,
        StatusId     INT              NOT NULL,
        SizeInBytes  BIGINT           NOT NULL,
        CreatedAt    DATETIMEOFFSET   NOT NULL,
        PublishedAt  DATETIMEOFFSET   NULL
    );

    CREATE INDEX IX_documentum_Documents_OwnerId ON documentum.Documents (OwnerId);
    CREATE INDEX IX_documentum_Documents_StatusId ON documentum.Documents (StatusId);
END
GO
