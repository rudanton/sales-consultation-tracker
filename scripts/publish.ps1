param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = ""
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

$versionProperties = @()
if (-not [string]::IsNullOrWhiteSpace($Version)) {
    $normalizedVersion = $Version.Trim().TrimStart("v", "V")
    $versionParts = $normalizedVersion.Split(".")
    while ($versionParts.Count -lt 4) {
        $versionParts += "0"
    }

    $assemblyVersion = ($versionParts | Select-Object -First 4) -join "."
    $versionProperties = @(
        "-p:Version=$normalizedVersion",
        "-p:AssemblyVersion=$assemblyVersion",
        "-p:FileVersion=$assemblyVersion"
    )
}

dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=false `
    -p:NuGetAudit=false `
    @versionProperties `
    --output $publishDirectory

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

if (Test-Path $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $publishDirectory "*") -DestinationPath $zipPath -Force

Write-Host ""
Write-Host "Done."
Write-Host "Zip: $zipPath"
