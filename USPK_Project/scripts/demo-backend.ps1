$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$envPath = Join-Path $projectRoot '.env.demo'
if (-not (Test-Path -LiteralPath $envPath -PathType Leaf)) {
    throw 'Missing .env.demo. Run demo-init.ps1 first.'
}

foreach ($line in (Get-Content -LiteralPath $envPath -Encoding UTF8)) {
    if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith('#')) { continue }
    if ($line -notmatch '^([A-Za-z_][A-Za-z0-9_]*)=(.*)$') {
        throw 'Invalid entry in .env.demo.'
    }
    $name = $Matches[1]
    $value = $Matches[2]
    if ($value.Length -ge 2 -and $value.StartsWith("'") -and $value.EndsWith("'")) {
        $value = $value.Substring(1, $value.Length - 2)
    }
    [System.Environment]::SetEnvironmentVariable($name, $value, 'Process')
}

Set-Location $projectRoot
& dotnet run --project USPK_Project/USPK_Project.csproj --no-launch-profile --urls http://127.0.0.1:5133
exit $LASTEXITCODE
