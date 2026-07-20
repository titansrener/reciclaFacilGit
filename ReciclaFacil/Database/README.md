# Banco de dados

Os scripts numerados formam o schema do `ReciclaFacilWeb` e são idempotentes. Para
criar ou atualizar o banco local no SQL Server Express, execute a partir da raiz:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Database\Apply-Database.ps1
```

Outra instância e um executável `sqlcmd` fora do `PATH` podem ser informados:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Database\Apply-Database.ps1 `
  -ServerInstance '.\SQLEXPRESS' `
  -SqlCmdPath 'C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\SQLCMD.EXE'
```

O executor exige autenticação integrada do Windows, confia no certificado local,
valida que a sequência não possui lacunas e interrompe na primeira falha. O script
`001` cria o banco quando necessário; os demais evoluem o schema sem apagar dados.
