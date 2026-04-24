-- Intranet module baseline schema.
-- Every object in this module lives under the intranet schema.

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'intranet')
    EXEC('CREATE SCHEMA intranet');
GO

IF OBJECT_ID('intranet.Users', 'U') IS NULL
BEGIN
    CREATE TABLE intranet.Users
    (
        Id             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        DisplayName    NVARCHAR(200)    NOT NULL,
        Email          NVARCHAR(320)    NOT NULL,
        IsActive       BIT              NOT NULL,
        CreatedAt      DATETIMEOFFSET   NOT NULL,
        DeactivatedAt  DATETIMEOFFSET   NULL,
        CONSTRAINT UQ_intranet_Users_Email UNIQUE (Email)
    );

    CREATE INDEX IX_intranet_Users_IsActive ON intranet.Users (IsActive);
END
GO

IF OBJECT_ID('intranet.Announcements', 'U') IS NULL
BEGIN
    CREATE TABLE intranet.Announcements
    (
        Id         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Title      NVARCHAR(200)    NOT NULL,
        Body       NVARCHAR(MAX)    NOT NULL,
        AuthorId   UNIQUEIDENTIFIER NOT NULL,
        CreatedAt  DATETIMEOFFSET   NOT NULL,
        CONSTRAINT FK_intranet_Announcements_Users
            FOREIGN KEY (AuthorId) REFERENCES intranet.Users (Id)
    );
END
GO
