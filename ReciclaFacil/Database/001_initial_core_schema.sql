SET NOCOUNT ON;

IF DB_ID(N'ReciclaFacilWeb') IS NULL
BEGIN
    CREATE DATABASE [ReciclaFacilWeb];
END;
GO

USE [ReciclaFacilWeb];
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        usuarioId              nvarchar(128) NOT NULL,
        Email                  nvarchar(256) NULL,
        EmailConfirmed         bit NOT NULL CONSTRAINT DF_Usuarios_EmailConfirmed DEFAULT (0),
        PasswordHash           nvarchar(max) NULL,
        SecurityStamp          nvarchar(max) NULL,
        PhoneNumber            nvarchar(max) NULL,
        PhoneNumberConfirmed   bit NOT NULL CONSTRAINT DF_Usuarios_PhoneConfirmed DEFAULT (0),
        TwoFactorEnabled       bit NOT NULL CONSTRAINT DF_Usuarios_TwoFactor DEFAULT (0),
        LockoutEndDateUtc      datetime NULL,
        LockoutEnabled         bit NOT NULL CONSTRAINT DF_Usuarios_LockoutEnabled DEFAULT (0),
        AccessFailedCount      int NOT NULL CONSTRAINT DF_Usuarios_AccessFailed DEFAULT (0),
        UserName               nvarchar(256) NOT NULL,
        dataCadastro           datetime NULL,
        ativo                  bit NULL,
        Discriminator          nvarchar(128) NOT NULL CONSTRAINT DF_Usuarios_Discriminator DEFAULT (N'ApplicationUser'),
        CONSTRAINT PK_Usuarios PRIMARY KEY (usuarioId)
    );

    CREATE UNIQUE INDEX UX_Usuarios_UserName ON dbo.Usuarios(UserName);
    CREATE INDEX IX_Usuarios_Email ON dbo.Usuarios(Email);
END;
GO

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        Id   nvarchar(128) NOT NULL,
        Name nvarchar(256) NOT NULL,
        CONSTRAINT PK_Roles PRIMARY KEY (Id)
    );

    CREATE UNIQUE INDEX UX_Roles_Name ON dbo.Roles(Name);
END;
GO

IF OBJECT_ID(N'dbo.UsuarioRole', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UsuarioRole
    (
        UserId          nvarchar(128) NOT NULL,
        RoleId          nvarchar(128) NOT NULL,
        IdentityUser_Id nvarchar(128) NULL,
        CONSTRAINT PK_UsuarioRole PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UsuarioRole_Usuarios FOREIGN KEY (UserId) REFERENCES dbo.Usuarios(usuarioId),
        CONSTRAINT FK_UsuarioRole_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.Claims', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Claims
    (
        Id              int IDENTITY(1,1) NOT NULL,
        UserId          nvarchar(128) NULL,
        ClaimType       nvarchar(max) NULL,
        ClaimValue      nvarchar(max) NULL,
        IdentityUser_Id nvarchar(128) NULL,
        CONSTRAINT PK_Claims PRIMARY KEY (Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.Logins', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Logins
    (
        LoginProvider   nvarchar(128) NOT NULL,
        ProviderKey     nvarchar(128) NOT NULL,
        UserId          nvarchar(128) NOT NULL,
        IdentityUser_Id nvarchar(128) NULL,
        CONSTRAINT PK_Logins PRIMARY KEY (LoginProvider, ProviderKey, UserId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Materiais', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Materiais
    (
        materialId             int IDENTITY(1,1) NOT NULL,
        descricao              varchar(50) NOT NULL,
        tempoMedioDecomposicao int NOT NULL,
        selecionado            bit NOT NULL CONSTRAINT DF_Materiais_Selecionado DEFAULT (0),
        CONSTRAINT PK_Materiais PRIMARY KEY (materialId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Cooperativas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cooperativas
    (
        cooperativaId      nvarchar(128) NOT NULL,
        cnpj               varchar(14) NOT NULL,
        razaoSocial        varchar(100) NOT NULL,
        endereco           varchar(150) NOT NULL,
        cidade             varchar(80) NOT NULL,
        estado             varchar(2) NOT NULL,
        enderecoCoordenada geometry NULL,
        CONSTRAINT PK_Cooperativas PRIMARY KEY (cooperativaId),
        CONSTRAINT FK_Cooperativas_Usuarios FOREIGN KEY (cooperativaId) REFERENCES dbo.Usuarios(usuarioId)
    );

    CREATE UNIQUE INDEX UX_Cooperativas_Cnpj ON dbo.Cooperativas(cnpj);
    CREATE INDEX IX_Cooperativas_Localizacao ON dbo.Cooperativas(estado, cidade);
END;
GO

IF OBJECT_ID(N'dbo.MateriaisComercializados', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MateriaisComercializados
    (
        materialId    int NOT NULL,
        cooperativaId nvarchar(128) NOT NULL,
        valorRevenda  decimal(8,2) NULL,
        CONSTRAINT PK_MateriaisComercializados PRIMARY KEY (materialId, cooperativaId),
        CONSTRAINT FK_MateriaisComercializados_Materiais
            FOREIGN KEY (materialId) REFERENCES dbo.Materiais(materialId),
        CONSTRAINT FK_MateriaisComercializados_Cooperativas
            FOREIGN KEY (cooperativaId) REFERENCES dbo.Cooperativas(cooperativaId)
    );
END;
GO
