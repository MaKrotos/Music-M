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

            // Сортируем релизы от новых к старым
            var sortedReleases = releases.OrderByDescending(r => r.TagName, new VersionComparer())
                                        .ToList();

            string osArchitecture = RuntimeInformation.OSArchitecture switch
            {
                Architecture.Arm64 => "ARM64",
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                _ => throw new Exception("Неизвестная архитектура")
            };

            // Проходим по всем релизам начиная с самого нового
            foreach (var release in sortedReleases)
            {
                // Проверяем, не старая ли это версия (текущая или младше)
                int comparisonResult = CompareVersions(release.TagName, currentVersion);
                if (comparisonResult <= 0)
                {
                    // Дошли до текущей или более старой версии, прекращаем поиск
                    break;
                }

                // Проверяем, есть ли в этом релизе нужные файлы
                var releaseInfo = CheckReleaseForAssets(release, osArchitecture, releases);
                if (releaseInfo != null)
                {
                    releaseInfo.IsNewVersionAvailable = true;
                    releaseInfo.Version = release.TagName;
                    releaseInfo.Name = release.Name;
                    releaseInfo.Body = release.Body;
                    return releaseInfo;
                }
            }

            return new ReleaseInfo { IsNewVersionAvailable = false };
        }

        private ReleaseInfo CheckReleaseForAssets(Octokit.Release release, string osArchitecture, IReadOnlyList<Octokit.Release> allReleases)
        {
            var releaseInfo = new ReleaseInfo();

            // Ищем MSIX пакеты
            var msixAsset = FindAssetForArchitecture(release.Assets, osArchitecture, ".msix");
            if (msixAsset != null)
            {
                releaseInfo.Assets[PackageType.MSIX] = new PackageAsset
                {
                    Url = msixAsset.BrowserDownloadUrl,
                    Size = msixAsset.Size,
                    Architecture = osArchitecture
                };
            }

            // Ищем ZIP архивы
            var zipAsset = FindAssetForArchitecture(release.Assets, osArchitecture, ".zip");
            if (zipAsset != null)
            {
                releaseInfo.Assets[PackageType.ZIP] = new PackageAsset
                {
                    Url = zipAsset.BrowserDownloadUrl,
                    Size = zipAsset.Size,
                    Architecture = osArchitecture
                };
            }

            // Ищем сертификат
            var cerAsset = FindCertificateAsset(allReleases, release.TagName);
            if (cerAsset != null)
            {
                releaseInfo.CertificateUrl = cerAsset.BrowserDownloadUrl;
            }

            // Если нашли хотя бы один пакет и сертификат, возвращаем
            if (releaseInfo.Assets.Count > 0 && !string.IsNullOrEmpty(releaseInfo.CertificateUrl))
            {
                return releaseInfo;
            }

            return null;
        }

        // Метод для сравнения версий
        private int CompareVersions(string version1, string version2)
        {
            if (version1 == version2)
                return 0;
            if (version1 == null)
                return -1;
            if (version2 == null)
                return 1;

            var parts1 = version1.Split('.');
            var parts2 = version2.Split('.');

            int maxLength = Math.Max(parts1.Length, parts2.Length);

            for (int i = 0; i < maxLength; i++)
            {
                int part1 = i < parts1.Length ? int.Parse(parts1[i]) : 0;
                int part2 = i < parts2.Length ? int.Parse(parts2[i]) : 0;

                if (part1 != part2)
                {
                    return part1.CompareTo(part2);
                }
            }

            return 0;
        }

        public class VersionComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return CompareVersions(x, y);
            }

            private int CompareVersions(string version1, string version2)
            {
                if (version1 == version2)
                    return 0;
                if (version1 == null)
                    return -1;
                if (version2 == null)
                    return 1;

                var parts1 = version1.Split('.');
                var parts2 = version2.Split('.');

                int maxLength = Math.Max(parts1.Length, parts2.Length);

                for (int i = 0; i < maxLength; i++)
                {
                    int part1 = i < parts1.Length ? int.Parse(parts1[i]) : 0;
                    int part2 = i < parts2.Length ? int.Parse(parts2[i]) : 0;

                    if (part1 != part2)
                    {
                        return part1.CompareTo(part2);
                    }
                }

                return 0;
            }
        }

        private Octokit.ReleaseAsset FindAssetForArchitecture(IReadOnlyList<Octokit.ReleaseAsset> assets, string architecture, string extension)
        {
            // Сначала ищем точное совпадение по архитектуре
            var asset = assets.FirstOrDefault(a =>
                a.Name.Contains(architecture, StringComparison.OrdinalIgnoreCase) &&
                a.Name.EndsWith(extension, StringComparison.OrdinalIgnoreCase));

            // Если не нашли, ищем любой файл с нужным расширением
            return asset ?? assets.FirstOrDefault(a =>
                a.Name.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
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
