using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using NuGet.Versioning;
using Squirrel;
using Squirrel.Sources;

namespace MusicX.Core.Helpers;

/// <summary> Describes a GitHub release, including attached assets. </summary>
[DataContract]
public class GithubRelease
{
    /// <summary> The name of this release. </summary>
    [DataMember(Name = "name")]
    public string Name { get; set; }

    /// <summary> True if this release is a prerelease. </summary>
    [DataMember(Name = "prerelease")]
    public bool Prerelease { get; set; }

    /// <summary> The date which this release was published publically. </summary>
    [DataMember(Name = "published_at")]
    public DateTime PublishedAt { get; set; }

    /// <summary> A list of assets (files) uploaded to this release. </summary>
    [DataMember(Name = "assets")]
    public GithubReleaseAsset[] Assets { get; set; }

    /// <summary> The name of tag tio which the release is created for. </summary>
    [DataMember(Name = "tag_name")]
    public string TagName { get; set; }
}

/// <summary> Describes a asset (file) uploaded to a GitHub release. </summary>
[DataContract]
public class GithubReleaseAsset
{
    /// <summary>
    ///     The asset URL for this release asset. Requests to this URL will use API
    ///     quota and return JSON unless the 'Accept' header is "application/octet-stream".
    /// </summary>
    [DataMember(Name = "url")]
    public string Url { get; set; }

    /// <summary>
    ///     The browser URL for this release asset. This does not use API quota,
    ///     however this URL only works for public repositories. If downloading
    ///     assets from a private repository, the <see cref="Url" /> property must
    ///     be used with an appropriate access token.
    /// </summary>
    [DataMember(Name = "browser_download_url")]
    public string BrowserDownloadUrl { get; set; }

    /// <summary> The name of this release asset. </summary>
    [DataMember(Name = "name")]
    public string Name { get; set; }

    /// <summary> The mime type of this release asset (as detected by GitHub). </summary>
    [DataMember(Name = "content_type")]
    public string ContentType { get; set; }
}

/// <summary>
///     Retrieves available releases from a GitHub repository. This class only
///     downloads assets from the very latest GitHub release.
/// </summary>
public class GithubSource : IUpdateSource
{
    /// <summary>
    ///     The URL of the GitHub repository to download releases from
    ///     (e.g. https://github.com/myuser/myrepo)
    /// </summary>
    public virtual Uri RepoUri { get; }

    /// <summary>
    ///     If true, the latest pre-release will be downloaded. If false, the latest
    ///     stable release will be downloaded.
    /// </summary>
    public virtual bool Prerelease { get; }

    /// <summary>
    ///     The file downloader used to perform HTTP requests.
    /// </summary>
    public virtual IFileDownloader Downloader { get; }

    /// <summary>
    ///     The GitHub releases which this class should download assets from when
    ///     executing <see cref="DownloadReleaseEntry" />. This property can be set
    ///     explicitly, otherwise it will also be set automatically when executing
    ///     <see cref="GetReleaseFeed(Guid?, ReleaseEntry)" />.
    /// </summary>
    public virtual ImmutableSortedDictionary<SemanticVersion, GithubRelease> Releases { get; set; }

    /// <summary>
    ///     The GitHub access token to use with the request to download releases.
    ///     If left empty, the GitHub rate limit for unauthenticated requests allows
    ///     for up to 60 requests per hour, limited by IP address.
    /// </summary>
    protected virtual string AccessToken { get; }

    /// <summary> The Bearer token used in the request. </summary>
    protected virtual string? Authorization => string.IsNullOrWhiteSpace(AccessToken) ? null : "Bearer " + AccessToken;

    /// <inheritdoc cref="GithubSource" />
    /// <param name="repoUrl">
    ///     The URL of the GitHub repository to download releases from
    ///     (e.g. https://github.com/myuser/myrepo)
    /// </param>
    /// <param name="accessToken">
    ///     The GitHub access token to use with the request to download releases.
    ///     If left empty, the GitHub rate limit for unauthenticated requests allows
    ///     for up to 60 requests per hour, limited by IP address.
    /// </param>
    /// <param name="prerelease">
    ///     If true, the latest pre-release will be downloaded. If false, the latest
    ///     stable release will be downloaded.
    /// </param>
    /// <param name="downloader">
    ///     The file downloader used to perform HTTP requests.
    /// </param>
    public GithubSource(string repoUrl, string accessToken, bool prerelease, IFileDownloader downloader)
    {
        RepoUri = new(repoUrl);
        AccessToken = accessToken;
        Prerelease = prerelease;
        Downloader = downloader;
    }

    /// <inheritdoc />
    public virtual async Task<ReleaseEntry[]> GetReleaseFeed(Guid? stagingId = null,
        ReleaseEntry? latestLocalRelease = null)
    {
        var releases = await GetReleases(Prerelease).ConfigureAwait(false);
        if (releases == null || !releases.Any())
            throw new($"No GitHub releases found at '{RepoUri}'.");

        // CS: we 'cache' the release here, so subsequent calls to DownloadReleaseEntry
        // will download assets from the same release in which we returned ReleaseEntry's
        // from. A better architecture would be to return an array of "GithubReleaseEntry"
        // containing a reference to the GithubReleaseAsset instead.
        Releases = releases.Where(b =>
                SemanticVersion.TryParse(b.TagName, out _) && b.Assets.Select(c => c.Name)
                    .Contains("RELEASES", StringComparer.OrdinalIgnoreCase))
            .ToImmutableSortedDictionary(b => SemanticVersion.Parse(b.TagName), b => b);

        return await Releases.Values.ToAsyncEnumerable().SelectManyAwait(async release =>
        {
            // this might be a browser url or an api url (depending on whether we have a AccessToken or not)
            // https://docs.github.com/en/rest/reference/releases#get-a-release-asset
            var assetUrl = GetAssetUrlFromName(release, "RELEASES");
            var releaseBytes = await Downloader.DownloadBytes(assetUrl, Authorization, "application/octet-stream")
                .ConfigureAwait(false);
            var txt = RemoveByteOrderMarkerIfPresent(releaseBytes);
            return ReleaseEntry.ParseReleaseFileAndApplyStaging(txt, stagingId).ToAsyncEnumerable();
        }).ToArrayAsync();
    }

    /// <inheritdoc />
    public virtual Task DownloadReleaseEntry(ReleaseEntry releaseEntry, string localFile, Action<int> progress)
    {
        if (Releases == null)
            throw new InvalidOperationException("No GitHub Release specified. Call GetReleaseFeed or set " +
                                                "GithubSource.Release before calling this function.");

        // this might be a browser url or an api url (depending on whether we have a AccessToken or not)
        // https://docs.github.com/en/rest/reference/releases#get-a-release-asset
        var assetUrl = GetAssetUrlFromName(Releases[releaseEntry.Version], releaseEntry.Filename);
        return Downloader.DownloadFile(assetUrl, localFile, progress, Authorization, "application/octet-stream");
    }

    /// <summary>
    ///     Retrieves a list of <see cref="GithubRelease" /> from the current repository.
    /// </summary>
    public virtual async Task<GithubRelease[]> GetReleases(bool includePrereleases, int perPage = 30, int page = 1)
    {
        // https://docs.github.com/en/rest/reference/releases
        var releasesPath = $"repos{RepoUri.AbsolutePath}/releases?per_page={perPage}&page={page}";
        var baseUri = GetApiBaseUrl(RepoUri);
        var getReleasesUri = new Uri(baseUri, releasesPath);
        var response = await Downloader
            .DownloadString(getReleasesUri.ToString(), Authorization, "application/vnd.github.v3+json")
            .ConfigureAwait(false);
        var releases = JsonConvert.DeserializeObject<List<GithubRelease>>(response)!;
        return releases.OrderByDescending(d => d.PublishedAt).Where(x => includePrereleases ? x.Prerelease : !x.Prerelease).ToArray();
    }

    /// <summary>
    ///     Given a <see cref="GithubRelease" /> and an asset filename (eg. 'RELEASES') this
    ///     function will return either <see cref="GithubReleaseAsset.BrowserDownloadUrl" /> or
    ///     <see cref="GithubReleaseAsset.Url" />, depending whether an access token is available
    ///     or not. Throws if the specified release has no matching assets.
    /// </summary>
    protected virtual string GetAssetUrlFromName(GithubRelease release, string assetName)
    {
        if (release.Assets == null || !release.Assets.Any())
            throw new ArgumentException($"No assets found in Github Release '{release.Name}'.");

        var allReleasesFiles = release.Assets
            .Where(a => a.Name.Equals(assetName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        if (allReleasesFiles == null || !allReleasesFiles.Any())
            throw new ArgumentException(
                $"Could not find asset called '{assetName}' in Github Release '{release.Name}'.");

        var asset = allReleasesFiles.First();

        return string.IsNullOrWhiteSpace(AccessToken)
            ?
            // if no AccessToken provided, we use the BrowserDownloadUrl which does not 
            // count towards the "unauthenticated api request" limit of 60 per hour per IP.
            asset.BrowserDownloadUrl
            :
            // otherwise, we use the regular asset url, which will allow us to retrieve
            // assets from private repositories
            // https://docs.github.com/en/rest/reference/releases#get-a-release-asset
            asset.Url;
    }

    /// <summary>
    ///     Given a repository URL (e.g. https://github.com/myuser/myrepo) this function
    ///     returns the API base for performing requests. (eg. "https://api.github.com/"
    ///     or http://internal.github.server.local/api/v3)
    /// </summary>
    /// <param name="repoUrl"></param>
    /// <returns></returns>
    protected virtual Uri GetApiBaseUrl(Uri repoUrl)
    {
        var baseAddress = repoUrl.Host.EndsWith("github.com", StringComparison.OrdinalIgnoreCase)
            ? new("https://api.github.com/")
            :
            // if it's not github.com, it's probably an Enterprise server
            // now the problem with Enterprise is that the API doesn't come prefixed
            // it comes suffixed so the API path of http://internal.github.server.local
            // API location is http://internal.github.server.local/api/v3
            new Uri($"{repoUrl.Scheme}{Uri.SchemeDelimiter}{repoUrl.Host}/api/v3/");
        // above ^^ notice the end slashes for the baseAddress, explained here: http://stackoverflow.com/a/23438417/162694
        return baseAddress;
    }

    private static string RemoveByteOrderMarkerIfPresent(byte[]? content)
    {
        var output = Array.Empty<byte>();

        if (content == null) goto done;

        Func<byte[], byte[], bool> matches = (bom, src) =>
        {
            if (src.Length < bom.Length) return false;

            return !bom.Where((chr, index) => src[index] != chr).Any();
        };

        var utf32Be = new byte[] { 0x00, 0x00, 0xFE, 0xFF };
        var utf32Le = new byte[] { 0xFF, 0xFE, 0x00, 0x00 };
        var utf16Be = new byte[] { 0xFE, 0xFF };
        var utf16Le = new byte[] { 0xFF, 0xFE };
        var utf8 = new byte[] { 0xEF, 0xBB, 0xBF };

        if (matches(utf32Be, content))
            output = new byte[content.Length - utf32Be.Length];
        else if (matches(utf32Le, content))
            output = new byte[content.Length - utf32Le.Length];
        else if (matches(utf16Be, content))
            output = new byte[content.Length - utf16Be.Length];
        else if (matches(utf16Le, content))
            output = new byte[content.Length - utf16Le.Length];
        else if (matches(utf8, content))
            output = new byte[content.Length - utf8.Length];
        else
            output = content;

        done:
        if (output.Length > 0) Buffer.BlockCopy(content!, content!.Length - output.Length, output, 0, output.Length);

        return Encoding.UTF8.GetString(output);
    }
}