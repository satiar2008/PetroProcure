<#
.SYNOPSIS
    Builds a versioned PetroProcure release for IIS testing.

.DESCRIPTION
    Publishes the API, Web and Worker (framework-dependent, win-x64) plus the self-contained
    single-file installer EXE, and lays everything out under a versioned release folder that is
    separate from the source tree:

        <OutputRoot>\PetroProcure-v<version>\
            installer\PetroProcure.Installer.exe   (+ install.config.example.json)
            api\        web\        worker\
            INSTALL-README.txt

    Run the produced installer/PetroProcure.Installer.exe on the target Windows Server as Administrator.

.PARAMETER Version
    Overrides the version (otherwise read from version.json at the repo root).

.PARAMETER OutputRoot
    Root folder for releases (default E:\PetroProcureReleases).

.EXAMPLE
    pwsh build/build-release.ps1
    pwsh build/build-release.ps1 -Version 0.2.0
#>
[CmdletBinding()]
param(
    [string]$Version,
    [string]$OutputRoot = 'E:\PetroProcureReleases',
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not $Version) {
    $versionFile = Join-Path $repoRoot 'version.json'
    if (-not (Test-Path $versionFile)) { throw "version.json not found at $versionFile" }
    $Version = (Get-Content $versionFile -Raw | ConvertFrom-Json).version
}
if (-not $Version) { throw 'Version could not be determined.' }

$releaseName = "PetroProcure-v$Version"
$releaseDir  = Join-Path $OutputRoot $releaseName

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  Building $releaseName" -ForegroundColor Cyan
Write-Host "  Output: $releaseDir" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan

if (Test-Path $releaseDir) {
    Write-Host "Cleaning existing release folder..." -ForegroundColor Yellow
    Remove-Item $releaseDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $releaseDir | Out-Null

function Publish-App {
    param([string]$Project, [string]$Dest, [switch]$SelfContained, [switch]$SingleFile)

    $publishArgs = @(
        'publish', (Join-Path $repoRoot $Project),
        '-c', 'Release',
        '-r', $Runtime,
        '-o', $Dest,
        '--nologo', '-v', 'quiet',
        "-p:Version=$Version"
    )
    if ($SelfContained) { $publishArgs += '--self-contained'; $publishArgs += 'true' }
    else                { $publishArgs += '--self-contained'; $publishArgs += 'false' }
    if ($SingleFile)    { $publishArgs += '-p:PublishSingleFile=true' }

    Write-Host ""
    Write-Host ">> dotnet publish $Project" -ForegroundColor Cyan
    & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) { throw "Publish failed for $Project" }
}

# --- Application payloads (framework-dependent: target server needs the ASP.NET Core 9 Hosting Bundle) ---
Publish-App -Project 'src/PetroProcure.Api/PetroProcure.Api.csproj'       -Dest (Join-Path $releaseDir 'api')
Publish-App -Project 'src/PetroProcure.Web/PetroProcure.Web.csproj'       -Dest (Join-Path $releaseDir 'web')
Publish-App -Project 'src/PetroProcure.Worker/PetroProcure.Worker.csproj' -Dest (Join-Path $releaseDir 'worker')

# --- Installer (self-contained single EXE: no SDK/runtime required to run the installer itself) ---
$installerDir = Join-Path $releaseDir 'installer'
Publish-App -Project 'tools/PetroProcure.Installer/PetroProcure.Installer.csproj' -Dest $installerDir -SelfContained -SingleFile

# Keep only the installer EXE (+ optional example answer file); drop publish debris.
Get-ChildItem $installerDir -File | Where-Object { $_.Name -ne 'PetroProcure.Installer.exe' } | Remove-Item -Force -ErrorAction SilentlyContinue
Copy-Item (Join-Path $repoRoot 'tools/PetroProcure.Installer/install.config.example.json') $installerDir -Force

# --- Install readme ---
$readme = @"
PetroProcure $Version - IIS test deployment
===========================================

Prerequisites on the Windows Server (the installer checks these and stops if missing):
  1. IIS (Web Server role) with the "Web Server" and "Management Tools" features.
  2. ASP.NET Core 9 Hosting Bundle (NOT just the Runtime):
        https://dotnet.microsoft.com/download/dotnet/9.0  (ASP.NET Core Runtime -> Hosting Bundle)
        After installing it, run:  iisreset
        IMPORTANT: Install the Hosting Bundle AFTER enabling the IIS role. If IIS was enabled later,
        the ANCM module (aspnetcorev2.dll) will be missing; re-run the Hosting Bundle installer and
        choose 'Repair' (or: dotnet-hosting-9.x.x-win.exe /repair), then: net stop was /y && net start w3svc
  3. A reachable SQL Server instance (the installer creates the database and seeds it).

To install:
  1. Copy this entire "$releaseName" folder to the server.
  2. Right-click  installer\PetroProcure.Installer.exe  ->  Run as administrator.
  3. Answer the prompts (install path, SQL connection, ports, admin password, etc.),
     or place an install.config.json next to the EXE for an unattended install
     (see installer\install.config.example.json).

What the installer does:
  - Verifies prerequisites.
  - Writes appsettings.Production.json for API / Web / Worker.
  - Creates the database, applies migrations and seeds roles, permissions and the 'admin' user.
  - Creates two IIS sites (API and Web) with "No Managed Code" app pools.
  - Installs the AI Worker as a Windows Service (if selected).

After install:
  - Web UI:  http://<server>:<web-port>
  - Sign in as 'admin' with the bootstrap password you entered.
  - Open the chosen ports in Windows Firewall for remote access.

Folder layout:
  installer\   self-contained installer EXE
  api\         published API site content
  web\         published Blazor Server site content
  worker\      published background worker (Windows Service)
"@
Set-Content -Path (Join-Path $releaseDir 'INSTALL-README.txt') -Value $readme -Encoding UTF8

Write-Host ""
Write-Host "==============================================" -ForegroundColor Green
Write-Host "  Release ready: $releaseDir" -ForegroundColor Green
Write-Host "==============================================" -ForegroundColor Green
Get-ChildItem $releaseDir | Select-Object Name, @{N='Type';E={ if ($_.PSIsContainer){'dir'}else{'file'} }} | Format-Table -AutoSize
