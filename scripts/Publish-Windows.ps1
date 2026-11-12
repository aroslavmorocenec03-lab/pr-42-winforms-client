$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$buildDir = Join-Path $projectRoot ('artifacts/' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$packageDir = Join-Path $buildDir 'PostService-Windows-x64'
New-Item -ItemType Directory -Force -Path $packageDir | Out-Null
dotnet publish (Join-Path $projectRoot 'PostServiceSolution/PostService.App/PostService.App.csproj') -c Release -r win-x64 --self-contained true -o (Join-Path $packageDir 'Client') -warnaserror
if ($LASTEXITCODE -ne 0) { throw 'Client publish failed' }
dotnet publish (Join-Path $projectRoot 'PostServiceSolution/PostService/PostService.csproj') -c Release -r win-x64 --self-contained true -o (Join-Path $packageDir 'Server') -warnaserror
if ($LASTEXITCODE -ne 0) { throw 'Server publish failed' }
@'
@echo off
cd /d "%~dp0"
start "PostService API" /D "%~dp0Server" "%~dp0Server\PostService.exe" --urls http://localhost:5211 --contentRoot "%~dp0Server"
timeout /t 2 /nobreak >nul
start "PostService Client" /D "%~dp0Client" "%~dp0Client\PostService.App.exe"
'@ | Set-Content -LiteralPath (Join-Path $packageDir 'Start.cmd') -Encoding Ascii
$zipPath = Join-Path $buildDir 'PostService-Windows-x64.zip'
Compress-Archive -Path (Join-Path $packageDir '*') -DestinationPath $zipPath -CompressionLevel Optimal
Write-Output $zipPath
