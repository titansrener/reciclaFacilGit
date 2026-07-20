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

## Backup

Para criar um backup operacional antes de uma implantação:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Database\Backup-Database.ps1
```

O arquivo recebe data e hora UTC e é gravado no diretório padrão de backups da
instância, onde a conta do serviço SQL Server já possui acesso. O comando usa
`COPY_ONLY` para não interferir em uma futura cadeia de backups, habilita checksum e
executa `RESTORE VERIFYONLY` antes de informar o caminho do arquivo.

Outra instância ou banco pode ser informado com `-ServerInstance` e `-DatabaseName`.
O parâmetro do banco aceita somente letras, números e sublinhado para impedir injeção
em comandos administrativos.
