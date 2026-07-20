USE [ReciclaFacilWeb];
GO

IF OBJECT_ID(N'dbo.Caminhoes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Caminhoes
    (
        caminhaoId    int IDENTITY(1,1) NOT NULL,
        descricao     varchar(45) NOT NULL,
        placa         varchar(8) NOT NULL,
        cooperativaId nvarchar(128) NOT NULL,
        CONSTRAINT PK_Caminhoes PRIMARY KEY (caminhaoId),
        CONSTRAINT FK_Caminhoes_Cooperativas FOREIGN KEY (cooperativaId)
            REFERENCES dbo.Cooperativas(cooperativaId)
    );
    CREATE UNIQUE INDEX UX_Caminhoes_Placa ON dbo.Caminhoes(placa);
    CREATE INDEX IX_Caminhoes_Cooperativa ON dbo.Caminhoes(cooperativaId);
END;
GO

IF OBJECT_ID(N'dbo.Funcionarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Funcionarios
    (
        funcionarioId nvarchar(128) NOT NULL,
        nome           varchar(45) NOT NULL,
        dataNascimento date NOT NULL,
        cooperativaId  nvarchar(128) NOT NULL,
        CONSTRAINT PK_Funcionarios PRIMARY KEY (funcionarioId),
        CONSTRAINT FK_Funcionarios_Usuarios FOREIGN KEY (funcionarioId)
            REFERENCES dbo.Usuarios(usuarioId),
        CONSTRAINT FK_Funcionarios_Cooperativas FOREIGN KEY (cooperativaId)
            REFERENCES dbo.Cooperativas(cooperativaId)
    );
    CREATE INDEX IX_Funcionarios_Cooperativa ON dbo.Funcionarios(cooperativaId);
END;
GO

IF OBJECT_ID(N'dbo.CaminhoesColetas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CaminhoesColetas
    (
        caminhaoId int NOT NULL,
        coletaId   int NOT NULL,
        CONSTRAINT PK_CaminhoesColetas PRIMARY KEY (caminhaoId, coletaId),
        CONSTRAINT FK_CaminhoesColetas_Caminhoes FOREIGN KEY (caminhaoId)
            REFERENCES dbo.Caminhoes(caminhaoId),
        CONSTRAINT FK_CaminhoesColetas_Coletas FOREIGN KEY (coletaId)
            REFERENCES dbo.Coletas(coletaId)
    );
    CREATE INDEX IX_CaminhoesColetas_Coleta ON dbo.CaminhoesColetas(coletaId);
END;
GO

IF OBJECT_ID(N'dbo.FuncionariosColetas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FuncionariosColetas
    (
        coletaId     int NOT NULL,
        funcionarioId nvarchar(128) NOT NULL,
        CONSTRAINT PK_FuncionariosColetas PRIMARY KEY (coletaId, funcionarioId),
        CONSTRAINT FK_FuncionariosColetas_Coletas FOREIGN KEY (coletaId)
            REFERENCES dbo.Coletas(coletaId),
        CONSTRAINT FK_FuncionariosColetas_Funcionarios FOREIGN KEY (funcionarioId)
            REFERENCES dbo.Funcionarios(funcionarioId)
    );
    CREATE INDEX IX_FuncionariosColetas_Funcionario ON dbo.FuncionariosColetas(funcionarioId);
END;
GO
