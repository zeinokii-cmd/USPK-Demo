$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$envPath = Join-Path $projectRoot '.env.demo'
$demoPassword = 'USPKDemo2026Only'

function New-RandomHex([int]$byteCount) {
    $bytes = New-Object byte[] $byteCount
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return ([System.BitConverter]::ToString($bytes)).Replace('-', '').ToLowerInvariant()
}

& docker info --format '{{.ServerVersion}}' *> $null
if ($LASTEXITCODE -ne 0) { throw 'Start Docker Desktop (Linux containers) and try again.' }
& docker compose version *> $null
if ($LASTEXITCODE -ne 0) { throw 'Docker Compose v2 is required.' }

if (-not (Test-Path -LiteralPath $envPath -PathType Leaf)) {
    $volumes = @(& docker volume ls --format '{{.Name}}')
    if ($LASTEXITCODE -ne 0) { throw 'Could not list Docker volumes.' }
    if ($volumes -contains 'uspk_teacher_demo_data') {
        throw 'Demo database already exists but .env.demo is missing. Restore that file, or run docker volume rm uspk_teacher_demo_data to reset this demo.'
    }
    $dbPassword = New-RandomHex 24
    $jwtKey = New-RandomHex 32
    $lines = @(
        "POSTGRES_PASSWORD=$dbPassword"
        'ASPNETCORE_ENVIRONMENT=Development'
        'Demo__Enabled=true'
        "Demo__SeedPassword=$demoPassword"
        "ConnectionStrings__Demo=Host=127.0.0.1;Port=55433;Database=uspk_demo;Username=uspk_demo;Password=$dbPassword"
        'Jwt__Issuer=uspk-demo'
        'Jwt__Audience=uspk-demo-api'
        "Jwt__SigningKey=$jwtKey"
        'Authorization__AdministratorRole=admin'
    )
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($envPath, (($lines -join "`n") + "`n"), $utf8NoBom)
}

Push-Location $projectRoot
try {
    & docker compose --env-file .env.demo -f compose.demo.yml up -d --wait
    if ($LASTEXITCODE -ne 0) { throw 'Could not start the isolated demo database. Check Docker Desktop and port 55433.' }
} finally { Pop-Location }

Write-Host 'Demo database ready on 127.0.0.1:55433.'
Write-Host "Demo password: $demoPassword"
