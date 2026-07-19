USE [ReciclaFacilWeb];
GO

IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        Id            uniqueidentifier NOT NULL,
        UserId        nvarchar(128) NOT NULL,
        TokenHash     char(64) NOT NULL,
        CreatedAtUtc  datetime2 NOT NULL,
        ExpiresAtUtc  datetime2 NOT NULL,
        RevokedAtUtc  datetime2 NULL,
        ReplacedByHash char(64) NULL,
        CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
        CONSTRAINT FK_RefreshTokens_Usuarios FOREIGN KEY (UserId) REFERENCES dbo.Usuarios(usuarioId)
    );
    CREATE UNIQUE INDEX UX_RefreshTokens_TokenHash ON dbo.RefreshTokens(TokenHash);
    CREATE INDEX IX_RefreshTokens_UserExpiry ON dbo.RefreshTokens(UserId, ExpiresAtUtc);
END;
GO
