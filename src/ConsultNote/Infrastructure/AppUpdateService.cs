using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace ConsultNote.Infrastructure;

public sealed class AppUpdateService
{
    public async Task<UpdateCheckResult> CheckLatestRelease()
    {
        return await new GitHubReleaseUpdateChecker().CheckLatestRelease(GetCurrentAppVersion());
    }

    public async Task<UpdatePrepareResult> PrepareUpdate(UpdateCheckResult update)
    {
        if (string.IsNullOrWhiteSpace(update.DownloadUrl))
        {
            return UpdatePrepareResult.Failed("업데이트 zip 파일을 찾을 수 없습니다.");
        }

        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath) ||
            !string.Equals(Path.GetFileName(processPath), "SalesConsultationTracker.exe", StringComparison.OrdinalIgnoreCase))
        {
            return UpdatePrepareResult.Failed("자동 업데이트는 배포용 SalesConsultationTracker.exe로 실행 중일 때만 사용할 수 있습니다.");
        }

        var updatesDirectory = Path.Combine(AppPaths.StorageDirectory, "updates");
        Directory.CreateDirectory(updatesDirectory);

        var assetName = string.IsNullOrWhiteSpace(update.AssetName)
            ? $"SalesConsultationTracker_{update.LatestVersion}.zip"
            : SanitizeFileName(update.AssetName);
        var zipPath = Path.Combine(updatesDirectory, assetName);
        await DownloadFile(update.DownloadUrl, zipPath);

        var scriptPath = Path.Combine(updatesDirectory, "apply-update.ps1");
        await File.WriteAllTextAsync(scriptPath, BuildUpdaterScript(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        var appDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var arguments =
            $"-NoProfile -ExecutionPolicy Bypass -File {Quote(scriptPath)} " +
            $"-ZipPath {Quote(zipPath)} " +
            $"-AppDir {Quote(appDirectory)} " +
            $"-ExePath {Quote(processPath)} " +
            $"-ProcessId {Environment.ProcessId}";

        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = appDirectory,
        });

        return UpdatePrepareResult.Success();
    }

    private static Version GetCurrentAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null
            ? new Version(0, 0, 0)
            : new Version(version.Major, version.Minor, version.Build);
    }

    private static async Task DownloadFile(string downloadUrl, string destinationPath)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ConsultNote", GetCurrentAppVersion().ToString()));

        using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var remoteStream = await response.Content.ReadAsStreamAsync();
        await using var localStream = File.Create(destinationPath);
        await remoteStream.CopyToAsync(localStream);
    }

    private static string BuildUpdaterScript()
    {
        return """
param(
    [Parameter(Mandatory = $true)][string]$ZipPath,
    [Parameter(Mandatory = $true)][string]$AppDir,
    [Parameter(Mandatory = $true)][string]$ExePath,
    [Parameter(Mandatory = $true)][int]$ProcessId
)

$ErrorActionPreference = "Stop"
$preserveNames = @("consultnote.db", "storage", "backup", "logs", "settings")
$logDir = Join-Path $AppDir "logs"
$extractDir = Join-Path ([System.IO.Path]::GetTempPath()) ("ConsultNoteUpdate_" + [System.Guid]::NewGuid().ToString("N"))

try {
    New-Item -ItemType Directory -Force -Path $logDir | Out-Null

    try {
        Wait-Process -Id $ProcessId -Timeout 30 -ErrorAction SilentlyContinue
    } catch {
    }

    New-Item -ItemType Directory -Force -Path $extractDir | Out-Null
    Expand-Archive -LiteralPath $ZipPath -DestinationPath $extractDir -Force

    Get-ChildItem -LiteralPath $extractDir -Force | ForEach-Object {
        if (-not ($preserveNames -contains $_.Name)) {
            $destination = Join-Path $AppDir $_.Name
            if ($_.PSIsContainer) {
                Copy-Item -LiteralPath $_.FullName -Destination $destination -Recurse -Force
            } else {
                Copy-Item -LiteralPath $_.FullName -Destination $destination -Force
            }
        }
    }

    Remove-Item -LiteralPath $ZipPath -Force -ErrorAction SilentlyContinue
    Start-Process -FilePath $ExePath -WorkingDirectory $AppDir
} catch {
    New-Item -ItemType Directory -Force -Path $logDir | Out-Null
    $_ | Out-String | Set-Content -LiteralPath (Join-Path $logDir "update-error.txt") -Encoding UTF8

    if (Test-Path -LiteralPath $ExePath) {
        Start-Process -FilePath $ExePath -WorkingDirectory $AppDir
    }
} finally {
    if (Test-Path -LiteralPath $extractDir) {
        Remove-Item -LiteralPath $extractDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
""";
    }

    private static string Quote(string value)
    {
        return $"'{value.Replace("'", "''", StringComparison.Ordinal)}'";
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidCharacter, '_');
        }

        return string.IsNullOrWhiteSpace(fileName) ? "SalesConsultationTracker_update.zip" : fileName;
    }
}

public sealed record UpdatePrepareResult(bool IsSuccess, string Message)
{
    public static UpdatePrepareResult Success()
    {
        return new UpdatePrepareResult(true, string.Empty);
    }

    public static UpdatePrepareResult Failed(string message)
    {
        return new UpdatePrepareResult(false, message);
    }
}
