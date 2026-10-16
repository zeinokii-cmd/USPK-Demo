$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$frontendRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\USPK_Project_Front\USPK_Project_Front')).Path
Set-Location $frontendRoot
$env:VITE_ADMIN_ROLE = 'admin'
& npm.cmd run dev -- --host 127.0.0.1
exit $LASTEXITCODE
