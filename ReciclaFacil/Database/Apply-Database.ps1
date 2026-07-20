[CmdletBinding()]
param(
    [string] $ServerInstance = '.\SQLEXPRESS',
    [string] $SqlCmdPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

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
$scripts = @(
    Get-ChildItem -LiteralPath $PSScriptRoot -File |
        Where-Object Name -Match '^\d{3}_.+\.sql$' |
        Sort-Object Name
)

if ($scripts.Count -eq 0) {
    throw "Nenhum script numerado foi encontrado em '$PSScriptRoot'."
}

for ($index = 0; $index -lt $scripts.Count; $index++) {
    $expectedPrefix = '{0:D3}_' -f ($index + 1)
    if (-not $scripts[$index].Name.StartsWith($expectedPrefix, [StringComparison]::Ordinal)) {
        throw "Sequência inválida: esperado um script iniciado por '$expectedPrefix', encontrado '$($scripts[$index].Name)'."
    }
}

Write-Host "Aplicando $($scripts.Count) scripts em '$ServerInstance'..."

foreach ($script in $scripts) {
    Write-Host "  $($script.Name)"
    & $sqlcmd `
        -S $ServerInstance `
        -E `
        -C `
        -b `
        -r 1 `
        -i $script.FullName

    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao executar '$($script.Name)' (código $LASTEXITCODE)."
    }
}

Write-Host 'Banco ReciclaFacilWeb atualizado com sucesso.'
