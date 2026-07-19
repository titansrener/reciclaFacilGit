USE [ReciclaFacilWeb];
GO

IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        clienteId          nvarchar(128) NOT NULL,
        cpf                varchar(11) NULL,
        tipo               char(1) NOT NULL,
        nome               varchar(75) NOT NULL,
        endereco           varchar(100) NOT NULL,
        enderecoCoordenada geometry NULL,
        email              varchar(45) NOT NULL,
        sexo               char(1) NOT NULL,
        dataNascimento     datetime NOT NULL,
        telefone           varchar(11) NULL,
        celular            varchar(11) NOT NULL,
        cooperativaId      nvarchar(128) NOT NULL,
        CONSTRAINT PK_Clientes PRIMARY KEY (clienteId),
        CONSTRAINT FK_Clientes_Usuarios FOREIGN KEY (clienteId) REFERENCES dbo.Usuarios(usuarioId),
        CONSTRAINT FK_Clientes_Cooperativas FOREIGN KEY (cooperativaId) REFERENCES dbo.Cooperativas(cooperativaId)
    );
    CREATE INDEX IX_Clientes_Cooperativa ON dbo.Clientes(cooperativaId);
END;
GO

IF OBJECT_ID(N'dbo.Coletas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Coletas
    (
        coletaId      int IDENTITY(1,1) NOT NULL,
        horaAgendada  datetime NULL,
        quantidade    float NULL,
        coletado      char(1) NOT NULL,
        cooperativaId nvarchar(128) NOT NULL,
        CONSTRAINT PK_Coletas PRIMARY KEY (coletaId),
        CONSTRAINT CK_Coletas_Status CHECK (coletado IN ('A','I','F')),
        CONSTRAINT FK_Coletas_Cooperativas FOREIGN KEY (cooperativaId) REFERENCES dbo.Cooperativas(cooperativaId)
    );
    CREATE INDEX IX_Coletas_Agenda ON dbo.Coletas(cooperativaId, horaAgendada);
END;
GO

IF OBJECT_ID(N'dbo.ClientesColetas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClientesColetas
    (
        coletaId    int NOT NULL,
        clienteId   nvarchar(128) NOT NULL,
        horaDaColeta datetime NULL,
        coletado    char(1) NULL,
        CONSTRAINT PK_ClientesColetas PRIMARY KEY (coletaId, clienteId),
        CONSTRAINT FK_ClientesColetas_Coletas FOREIGN KEY (coletaId) REFERENCES dbo.Coletas(coletaId),
        CONSTRAINT FK_ClientesColetas_Clientes FOREIGN KEY (clienteId) REFERENCES dbo.Clientes(clienteId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Carteiras', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Carteiras
    (
        carteiraId             int IDENTITY(1,1) NOT NULL,
        saldo                 decimal(8,2) NOT NULL,
        dataUltimaMovimentacao datetime NULL,
        clienteId              nvarchar(128) NOT NULL,
        CONSTRAINT PK_Carteiras PRIMARY KEY (carteiraId),
        CONSTRAINT FK_Carteiras_Clientes FOREIGN KEY (clienteId) REFERENCES dbo.Clientes(clienteId)
    );
    CREATE INDEX IX_Carteiras_ClienteData ON dbo.Carteiras(clienteId, dataUltimaMovimentacao DESC);
END;
GO

IF OBJECT_ID(N'dbo.MateriaisColetados', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MateriaisColetados
    (
        materialId int NOT NULL,
        coletaId   int NOT NULL,
        clienteId  nvarchar(128) NOT NULL,
        quantidade float NULL,
        valorCompra decimal(18,0) NULL,
        coletado   char(1) NULL,
        CONSTRAINT PK_MateriaisColetados PRIMARY KEY (materialId, coletaId, clienteId),
        CONSTRAINT FK_MateriaisColetados_Materiais FOREIGN KEY (materialId) REFERENCES dbo.Materiais(materialId),
        CONSTRAINT FK_MateriaisColetados_ClientesColetas
            FOREIGN KEY (coletaId, clienteId) REFERENCES dbo.ClientesColetas(coletaId, clienteId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Notificacoes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notificacoes
    (
        notificacaoId int IDENTITY(1,1) NOT NULL,
        clienteId     nvarchar(128) NOT NULL,
        coletaId      int NOT NULL,
        cooperativaId nvarchar(128) NOT NULL,
        dataHorario   datetime NULL,
        ativa         bit NULL,
        descricao     varchar(150) NOT NULL,
        tipo          char(1) NOT NULL,
        CONSTRAINT PK_Notificacoes PRIMARY KEY (notificacaoId, clienteId),
        CONSTRAINT FK_Notificacoes_Clientes FOREIGN KEY (clienteId) REFERENCES dbo.Clientes(clienteId),
        CONSTRAINT FK_Notificacoes_Coletas FOREIGN KEY (coletaId) REFERENCES dbo.Coletas(coletaId),
        CONSTRAINT FK_Notificacoes_Cooperativas FOREIGN KEY (cooperativaId) REFERENCES dbo.Cooperativas(cooperativaId)
    );
    CREATE INDEX IX_Notificacoes_ClienteAtiva ON dbo.Notificacoes(clienteId, ativa, dataHorario DESC);
END;
GO
