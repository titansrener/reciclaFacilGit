[CmdletBinding()]
param(
    [string] $ServerInstance = '.\SQLEXPRESS',
    [string] $DatabaseName = 'ReciclaFacilWeb',
    [string] $SqlCmdPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($DatabaseName -notmatch '^[A-Za-z0-9_]+$') {
    throw 'DatabaseName aceita somente letras, números e sublinhado.'
}

function Resolve-SqlCmd {
    param([string] $ConfiguredPath)

    if ($ConfiguredPath) {
        if (-not (Test-Path -LiteralPath $ConfiguredPath -PathType Leaf)) {
            throw "sqlcmd não encontrado em '$ConfiguredPath'."
        }

        return (Resolve-Path -LiteralPath $ConfiguredPath).Path
    }

    $command = Get-Command sqlcmd -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $candidates = @(
        (Join-Path $env:ProgramFiles 'Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\SQLCMD.EXE'),
        (Join-Path $env:ProgramFiles 'Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE')
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return $candidate
        }
    }

    throw 'sqlcmd não foi encontrado. Instale as ferramentas de linha de comando do SQL Server ou informe -SqlCmdPath.'
}

$sqlcmd = Resolve-SqlCmd $SqlCmdPath
$databaseLiteral = $DatabaseName.Replace("'", "''")

$query = @"
SET NOCOUNT ON;

IF DB_ID(N'$databaseLiteral') IS NULL
    THROW 51000, 'O banco informado não existe.', 1;

DECLARE @directory nvarchar(4000) =
    CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultBackupPath'));

IF NULLIF(@directory, N'') IS NULL
    THROW 51001, 'O SQL Server não informou um diretório padrão de backup.', 1;

IF RIGHT(@directory, 1) NOT IN (N'\', N'/')
    SET @directory += N'\';

DECLARE @file nvarchar(4000) =
    @directory + N'$databaseLiteral' + N'_' +
    CONVERT(char(8), SYSUTCDATETIME(), 112) + N'_' +
    REPLACE(CONVERT(char(8), SYSUTCDATETIME(), 108), ':', '') + N'.bak';

BEGIN TRY
    DECLARE @backup nvarchar(max) =
        N'BACKUP DATABASE ' + QUOTENAME(N'$databaseLiteral') +
        N' TO DISK = @path WITH COPY_ONLY, CHECKSUM, INIT;';
    EXEC sys.sp_executesql @backup, N'@path nvarchar(4000)', @path = @file;

    DECLARE @verify nvarchar(max) =
        N'RESTORE VERIFYONLY FROM DISK = @path WITH CHECKSUM;';
    EXEC sys.sp_executesql @verify, N'@path nvarchar(4000)', @path = @file;

    SELECT @file AS VerifiedBackupPath;
END TRY
BEGIN CATCH
    THROW;
END CATCH;
"@

Write-Host "Criando backup verificado de '$DatabaseName' em '$ServerInstance'..."
& $sqlcmd `
    -S $ServerInstance `
    -E `
    -C `
    -b `
    -r 1 `
    -d master `
    -Q $query

if ($LASTEXITCODE -ne 0) {
    throw "Falha ao criar ou verificar o backup (código $LASTEXITCODE)."
}
