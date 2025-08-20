using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Text;
using SetupLib.Interfaces;
using SetupLib.Services;

namespace SetupLib
{
    public class AppUpdater
    {
        private readonly IGitHubClientService _gitHubClientService;
        private readonly IFileDownloadService _fileDownloadService;
        private readonly ISystemService _systemService;
        private readonly IInstallationService _installationService;
        public PackageType SelectedPackageType { get; set; } = PackageType.MSIX;
        public ReleaseInfo _currentReleaseInfo;


        public string currentVersion;
        public string version;
        public string Name { get; private set; }
        public string Tit { get; private set; }
        public string date { get; private set; }
        public int sizeFile => GetSelectedPackage()?.Size ?? 0;
        public string UriDownload => GetSelectedPackage()?.Url;
        public string UriDownloadMSIX => GetSelectedPackage()?.Url;

        private PackageAsset GetSelectedPackage()
        {
            return _currentReleaseInfo?.Assets.GetValueOrDefault(SelectedPackageType);
        }


        public AppUpdater(string currentVersion)
        {
            _gitHubClientService = new GitHubClientService();
            _fileDownloadService = new FileDownloadService();
            _systemService = new SystemService();
            _installationService = new InstallationService(_fileDownloadService, _systemService);

            this.currentVersion = currentVersion;

            // Подписка на события
            _fileDownloadService.DownloadProgressChanged += (sender, e) =>
                DownloadProgressChanged?.Invoke(sender, e);

            _installationService.InstallStatusChanged += (sender, e) =>
                InstallStatusChanged?.Invoke(sender, e);
        }

        public delegate void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e);
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        public delegate void InstallStatusChangedEventHandler(object sender, InstallStatusChangedEventArgs e);
        public event InstallStatusChangedEventHandler InstallStatusChanged;

        public async Task<bool> CheckForUpdates()
        {
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Проверка наличия обновлений..." });

            _currentReleaseInfo = await _gitHubClientService.GetLatestReleaseInfo("MaKrotos", "Music-M", currentVersion);

            if (_currentReleaseInfo == null)
            {
                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Не найдено подходящих релизов." });
                return false;
            }

            if (!_currentReleaseInfo.IsNewVersionAvailable)
            {
                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Установлена последняя версия: {_currentReleaseInfo.Version}" });
                return false;
            }

            this.version = _currentReleaseInfo.Version;
            this.Name = _currentReleaseInfo.Name;
            this.Tit = _currentReleaseInfo.Body;

            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs
            {
                Status = $"Найдена новая версия: {_currentReleaseInfo.Version} ({_systemService.GetOSArchitecture()})\n" +
                        $"Доступные форматы: {string.Join(", ", _currentReleaseInfo.Assets.Keys)}"
            });

            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Информация о релизе получена успешно." });

            return true;
        }

        public List<PackageType> GetAvailablePackageTypes()
        {
            return _currentReleaseInfo?.Assets.Keys.ToList() ?? new List<PackageType>();
        }



        public async Task DownloadAndOpenFile(bool skip = false, bool forceInstall = false, string? PathInstallZIP = null)
        {
            try
            {
                await _installationService.InstallUpdateAsync(this, skip, forceInstall, PathInstallZIP);
            }
            catch (Exception ex)
            {
                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Ошибка: {ex.Message}" });
                throw;
            }
        }

        public bool IsVersionInstalled(string targetVersion)
        {
            return _systemService.IsDotNetVersionInstalled(targetVersion);
        }

        public List<string> GetInstalledDotNetVersions()
        {
            return _systemService.GetInstalledDotNetVersions();
        }

        public bool IsAppInstalled(string appName)
        {
            return _systemService.IsAppInstalled(appName);
        }

        public void InstallLatestDotNetAppRuntime()
        {
            _installationService.InstallLatestDotNetRuntime();
        }

        public bool CheckIfWingetIsInstalled()
        {
            return _systemService.CheckIfWingetIsInstalled();
        }
    }

    public class DownloadProgressChangedEventArgs : EventArgs
    {
        public long TotalBytes { get; set; }
        public long BytesDownloaded { get; set; }
        public double Percentage { get; set; }
    }

    public class InstallStatusChangedEventArgs : EventArgs
    {
        public string Status { get; set; }
    }
}