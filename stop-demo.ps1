$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$projectRoot = Join-Path $PSScriptRoot 'USPK_Project'
$envPath = Join-Path $projectRoot '.env.demo'
if (-not (Test-Path -LiteralPath $envPath -PathType Leaf)) { throw 'Missing .env.demo.' }
Push-Location $projectRoot
try {
    & docker compose --env-file .env.demo -f compose.demo.yml down
    if ($LASTEXITCODE -ne 0) { throw 'Could not stop the demo database.' }
} finally { Pop-Location }
Write-Host 'Demo database stopped. Close the backend and frontend windows with Ctrl+C.'
