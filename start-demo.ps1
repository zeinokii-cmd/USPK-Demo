$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = $PSScriptRoot
$backend = Join-Path $root 'USPK_Project\scripts\demo-backend.ps1'
$frontend = Join-Path $root 'USPK_Project\scripts\demo-frontend.ps1'
$frontendRoot = Join-Path $root 'USPK_Project_Front\USPK_Project_Front'

foreach ($command in @('docker', 'dotnet', 'node', 'npm.cmd')) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        throw "Missing $command. Install the prerequisites listed in README.md."
    }
}
& dotnet --list-sdks | Out-String | ForEach-Object {
    if ($_ -notmatch '(?m)^10\.') { throw '.NET 10 SDK is required.' }
}
& node --version | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Node.js is required.' }

& (Join-Path $root 'USPK_Project\scripts\demo-init.ps1')
Push-Location $frontendRoot
try {
    & npm.cmd ci
    if ($LASTEXITCODE -ne 0) { throw 'Frontend dependency installation failed.' }
} finally { Pop-Location }

Start-Process -FilePath 'powershell.exe' -ArgumentList @('-NoExit', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', ('"' + $backend + '"')) -WorkingDirectory $root
Start-Process -FilePath 'powershell.exe' -ArgumentList @('-NoExit', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', ('"' + $frontend + '"')) -WorkingDirectory $root
Write-Host 'Backend and frontend started in separate PowerShell windows.'
Write-Host 'Open http://127.0.0.1:5173/#login after both report ready.'
