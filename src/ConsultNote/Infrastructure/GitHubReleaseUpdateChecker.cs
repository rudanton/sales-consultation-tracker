using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ConsultNote.Infrastructure;

public sealed class GitHubReleaseUpdateChecker
{
    private const string LatestReleaseUrl = "https://api.github.com/repos/rudanton/sales-consultation-tracker/releases/latest";

    public async Task<UpdateCheckResult> CheckLatestRelease(Version currentVersion)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ConsultNote", currentVersion.ToString()));

        using var response = await httpClient.GetAsync(LatestReleaseUrl);
        if (!response.IsSuccessStatusCode)
        {
            return UpdateCheckResult.Failed($"GitHub Release 정보를 확인할 수 없습니다. ({(int)response.StatusCode})");
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        var release = await JsonSerializer.DeserializeAsync<GitHubReleaseResponse>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        if (release is null || string.IsNullOrWhiteSpace(release.TagName))
        {
            return UpdateCheckResult.Failed("최신 버전 정보를 읽을 수 없습니다.");
        }

        var latestVersionText = release.TagName.Trim().TrimStart('v', 'V');
        if (!Version.TryParse(latestVersionText, out var latestVersion))
        {
            return UpdateCheckResult.Failed($"Release 버전 형식을 해석할 수 없습니다. ({release.TagName})");
        }

        var asset = release.Assets
            .Where(asset => !string.IsNullOrWhiteSpace(asset.BrowserDownloadUrl))
            .Where(asset => asset.Name?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == true)
            .OrderByDescending(asset => asset.Name?.Contains("SalesConsultationTracker", StringComparison.OrdinalIgnoreCase) == true)
            .FirstOrDefault();

        return new UpdateCheckResult(
            IsSuccess: true,
            HasUpdate: latestVersion > currentVersion,
            LatestVersion: latestVersion,
            ReleaseUrl: release.HtmlUrl,
            DownloadUrl: asset?.BrowserDownloadUrl,
            AssetName: asset?.Name,
            Message: string.Empty);
    }

    private sealed class GitHubReleaseResponse
    {
        public string? TagName { get; set; }

        public string? HtmlUrl { get; set; }

        public List<GitHubReleaseAssetResponse> Assets { get; set; } = [];
    }

    private sealed class GitHubReleaseAssetResponse
    {
        public string? Name { get; set; }

        public string? BrowserDownloadUrl { get; set; }
    }
}

public sealed record UpdateCheckResult(
    bool IsSuccess,
    bool HasUpdate,
    Version? LatestVersion,
    string? ReleaseUrl,
    string? DownloadUrl,
    string? AssetName,
    string Message)
{
    public static UpdateCheckResult Failed(string message)
    {
        return new UpdateCheckResult(false, false, null, null, null, null, message);
    }
}
