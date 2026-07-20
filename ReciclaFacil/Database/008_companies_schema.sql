USE [ReciclaFacilWeb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Empresas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Empresas
    (
        empresaId           nvarchar(128) NOT NULL,
        cnpj                varchar(14) NOT NULL,
        razaoSocial         varchar(150) NOT NULL,
        endereco            varchar(150) NOT NULL,
        enderecoCoordenada  geometry NULL,
        telefone            varchar(11) NOT NULL,
        fax                 varchar(25) NULL,
        email               varchar(45) NOT NULL,
        CONSTRAINT PK_Empresas PRIMARY KEY (empresaId),
        CONSTRAINT FK_Empresas_Usuarios
            FOREIGN KEY (empresaId) REFERENCES dbo.Usuarios(usuarioId)
    );

    CREATE UNIQUE INDEX UX_Empresas_Cnpj ON dbo.Empresas(cnpj);
END;
GO
