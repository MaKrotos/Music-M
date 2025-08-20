using SetupLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SetupLib.Services
{
    public class GitHubClientService : IGitHubClientService
    {
        private readonly Octokit.GitHubClient _client;

        public GitHubClientService()
        {
            _client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("Music-M"));
        }

        public async Task<ReleaseInfo> GetLatestReleaseInfo(string owner, string repo, string currentVersion)
        {
            var releases = await _client.Repository.Release.GetAll(owner, repo);

            foreach (var release in releases)
            {
                if (string.Compare(release.TagName, currentVersion) <= 0)
                {
                    return new ReleaseInfo { IsNewVersionAvailable = false, Version = release.TagName };
                }

                string osArchitecture = RuntimeInformation.OSArchitecture switch
                {
                    Architecture.Arm64 => "ARM64",
                    Architecture.X64 => "x64",
                    Architecture.X86 => "x86",
                    _ => throw new Exception("Неизвестная архитектура")
                };

                var msixAsset = release.Assets.FirstOrDefault(asset =>
                    asset.Name.Contains(osArchitecture) && asset.Name.EndsWith(".msix")) ??
                    release.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".msixbundle"));

                if (msixAsset != null)
                {
                    var cerAsset = FindCertificateAsset(releases, release.TagName);

                    if (cerAsset != null)
                    {
                        return new ReleaseInfo
                        {
                            IsNewVersionAvailable = true,
                            Version = release.TagName,
                            Name = release.Name,
                            Body = release.Body,
                            Size = msixAsset.Size,
                            CertificateUrl = cerAsset.BrowserDownloadUrl,
                            MsixUrl = msixAsset.BrowserDownloadUrl
                        };
                    }
                }
            }

            return null;
        }

        private Octokit.ReleaseAsset FindCertificateAsset(IReadOnlyList<Octokit.Release> releases, string currentReleaseTag)
        {
            var cerAsset = releases.FirstOrDefault(r => r.TagName == currentReleaseTag)?
                .Assets.FirstOrDefault(asset => asset.Name.EndsWith(".cer"));

            if (cerAsset != null)
                return cerAsset;

            foreach (var oldRelease in releases.Where(r => string.Compare(r.TagName, currentReleaseTag) < 0))
            {
                cerAsset = oldRelease.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".cer"));
                if (cerAsset != null)
                    return cerAsset;
            }

            return null;
        }
    }
}
