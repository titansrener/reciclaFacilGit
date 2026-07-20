USE [ReciclaFacilWeb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.PasswordResetTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens
    (
        Id           uniqueidentifier NOT NULL,
        UserId       nvarchar(128) NOT NULL,
        TokenHash    char(64) NOT NULL,
        CreatedAtUtc datetime2 NOT NULL,
        ExpiresAtUtc datetime2 NOT NULL,
        UsedAtUtc    datetime2 NULL,
        CONSTRAINT PK_PasswordResetTokens PRIMARY KEY (Id),
        CONSTRAINT FK_PasswordResetTokens_Usuarios
            FOREIGN KEY (UserId) REFERENCES dbo.Usuarios(usuarioId)
    );
    CREATE UNIQUE INDEX UX_PasswordResetTokens_TokenHash
        ON dbo.PasswordResetTokens(TokenHash);
    CREATE INDEX IX_PasswordResetTokens_UserExpiry
        ON dbo.PasswordResetTokens(UserId, ExpiresAtUtc);
END;
GO
