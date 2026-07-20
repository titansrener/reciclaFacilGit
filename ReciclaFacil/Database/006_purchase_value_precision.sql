USE [ReciclaFacilWeb];
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns c
    JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = N'MateriaisColetados'
      AND c.name = N'valorCompra'
      AND (c.precision <> 18 OR c.scale <> 2)
)
BEGIN
    ALTER TABLE dbo.MateriaisColetados
        ALTER COLUMN valorCompra decimal(18,2) NULL;
END;
GO
