USE [ReciclaFacilWeb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Clientes')
      AND name = N'UX_Clientes_Cpf'
)
BEGIN
    CREATE UNIQUE INDEX UX_Clientes_Cpf
        ON dbo.Clientes(cpf)
        WHERE cpf IS NOT NULL;
END;
GO
