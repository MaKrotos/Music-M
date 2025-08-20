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

        public string currentVersion;
        public string version;
        public string Name { get; private set; }
        public string Tit { get; private set; }
        public string date { get; private set; }
        public int sizeFile;
        public string UriDownload { get; private set; }
        public string UriDownloadMSIX { get; private set; }

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

            var releaseInfo = await _gitHubClientService.GetLatestReleaseInfo("MaKrotos", "Music-M", currentVersion);

            if (releaseInfo == null)
            {
                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "В папке релиза нет файла msixAsset." });
                return false;
            }

            if (!releaseInfo.IsNewVersionAvailable)
            {
                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Установлена последняя версия: {releaseInfo.Version}" });
                return false;
            }

            this.version = releaseInfo.Version;
            this.Name = releaseInfo.Name;
            this.Tit = releaseInfo.Body;
            this.sizeFile = releaseInfo.Size;
            this.UriDownload = releaseInfo.CertificateUrl;
            this.UriDownloadMSIX = releaseInfo.MsixUrl;

            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Найдена новая версия: {releaseInfo.Version} ({_systemService.GetOSArchitecture()})" });
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Информация о релизе получена успешно." });

            return true;
        }

        public async Task DownloadAndOpenFile(bool skip = false, bool forceInstall = false)
        {
            try
            {
                await _installationService.InstallUpdateAsync(this, skip, forceInstall);
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