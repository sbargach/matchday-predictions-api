CREATE TABLE dbo.Leagues
(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Leagues PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Code        NVARCHAR(20)  NOT NULL CONSTRAINT UX_Leagues_Code UNIQUE, -- simple join code
    CreatedUtc  DATETIME2(0)  NOT NULL CONSTRAINT DF_Leagues_CreatedUtc DEFAULT (SYSUTCDATETIME())
);
GO
