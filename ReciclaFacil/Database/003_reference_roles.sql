USE [ReciclaFacilWeb];
GO

MERGE dbo.Roles AS target
USING (VALUES
    (N'0', N'Admin'),
    (N'1', N'Cliente'),
    (N'2', N'Cooperativa'),
    (N'3', N'Empresa'),
    (N'4', N'Funcionario')
) AS source(Id, Name)
ON target.Id = source.Id
WHEN MATCHED AND target.Name <> source.Name THEN
    UPDATE SET Name = source.Name
WHEN NOT MATCHED THEN
    INSERT (Id, Name) VALUES (source.Id, source.Name);
GO
