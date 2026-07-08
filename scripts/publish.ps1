param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDirectory = Resolve-Path (Join-Path $scriptDirectory "..")
$projectPath = Join-Path $rootDirectory "src\ConsultNote\ConsultNote.csproj"
$distDirectory = Join-Path $rootDirectory "dist"
$publishDirectory = Join-Path $distDirectory "publish"

$timestamp = Get-Date -Format "yyyyMMdd_HHmm"
$zipPath = Join-Path $distDirectory "SalesConsultationTracker_$timestamp.zip"

Write-Host "Publishing SalesConsultationTracker..."

New-Item -ItemType Directory -Force -Path $distDirectory | Out-Null

$resolvedDist = Resolve-Path $distDirectory
if ((Test-Path $publishDirectory) -and ((Resolve-Path $publishDirectory).Path -like "$($resolvedDist.Path)*")) {
    Remove-Item -LiteralPath $publishDirectory -Recurse -Force
}

dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=false `
    -p:NuGetAudit=false `
    --output $publishDirectory

if (Test-Path $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $publishDirectory "*") -DestinationPath $zipPath -Force

Write-Host ""
Write-Host "Done."
Write-Host "Zip: $zipPath"
