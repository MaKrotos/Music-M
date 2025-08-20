using SetupLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SetupLib.Services
{
    public class InstallationService : IInstallationService
    {
        private readonly IFileDownloadService _fileDownloadService;
        private readonly ISystemService _systemService;

        public event EventHandler<InstallStatusChangedEventArgs> InstallStatusChanged;

        public InstallationService(IFileDownloadService fileDownloadService, ISystemService systemService)
        {
            _fileDownloadService = fileDownloadService;
            _systemService = systemService;
        }

        public async Task InstallUpdateAsync(AppUpdater appUpdater, bool skipCertCheck, bool forceInstall)
        {
            if (!skipCertCheck)
            {
                await HandleCertificateInstallation(appUpdater.UriDownload);
                await HandleDependenciesInstallation(forceInstall);
            }

            await InstallApplicationAsync(appUpdater.UriDownloadMSIX, forceInstall);
        }

        private async Task HandleCertificateInstallation(string certificateUrl)
        {
            OnInstallStatusChanged("Проверка прав администратора...");

            if (_systemService.IsRunningAsAdministrator())
            {
                OnInstallStatusChanged("Скачивание и установка сертификата...");
                await InstallCertificateAsync(certificateUrl);
                OnInstallStatusChanged("Сертификат установлен.");
            }
        }

        private async Task HandleDependenciesInstallation(bool forceInstall)
        {
            if (!forceInstall && _systemService.IsMicrosoftStoreInstalled())
            {
                OnInstallStatusChanged("Обнаружен Microsoft Store. Пропускаем установку зависимостей.");
                return;
            }

            OnInstallStatusChanged("Проверка наличия AppInstaller...");
            if (!_systemService.IsAppInstalled("AppInstaller") || forceInstall)
            {
                OnInstallStatusChanged("Скачивание и установка AppInstaller...");
                await InstallAppInstallerAsync();
                OnInstallStatusChanged("AppInstaller установлен.");
            }

            OnInstallStatusChanged("Проверка наличия WindowsAppRuntime...");
            if (!_systemService.IsAppInstalled("WindowsAppRuntime") || forceInstall)
            {
                OnInstallStatusChanged("Скачивание и установка WindowsAppRuntime...");
                await InstallAppRuntimeAsync();
                OnInstallStatusChanged("WindowsAppRuntime установлен.");
            }
        }

        public async Task InstallAppInstallerAsync()
        {
            var uri = "https://github.com/microsoft/winget-cli/releases/download/v1.7.10661/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle";
            var destinationPath = Path.Combine(Path.GetTempPath(), "appinstaller_" + Path.GetFileName(uri));

            await _fileDownloadService.DownloadFileAsync(uri, destinationPath);
            ExecutePowerShellCommand($"Add-AppxPackage -Path \"{destinationPath}\"");
        }

        public async Task InstallCertificateAsync(string certificateUrl)
        {
            var destinationPath = Path.Combine(Path.GetTempPath(), "cert_" + Path.GetFileName(certificateUrl));
            await _fileDownloadService.DownloadFileAsync(certificateUrl, destinationPath);

            X509Certificate2 cert = new X509Certificate2(destinationPath);
            using (X509Store store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadWrite);
                bool certificateExists = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false).Count > 0;

                if (!certificateExists || cert.NotAfter <= DateTime.Now)
                {
                    store.Add(cert);
                    OnInstallStatusChanged("Сертификат добавлен в хранилище.");
                }
                else
                {
                    OnInstallStatusChanged("Сертификат уже существует и действителен.");
                }
            }
        }

        public async Task InstallAppRuntimeAsync()
        {
            var runtimeUri = _systemService.GetOSArchitectureURI();
            var destinationPath = Path.Combine(Path.GetTempPath(), "runtime_" + Path.GetFileName(runtimeUri.ToString()));

            await _fileDownloadService.DownloadFileAsync(runtimeUri.ToString(), destinationPath);
            ExecutePowerShellCommand($"Add-AppxPackage -Path \"{destinationPath}\"");
        }

        private async Task InstallApplicationAsync(string msixUrl, bool forceInstall)
        {
            OnInstallStatusChanged("Скачивание и установка обновления...");

            var destinationPath = Path.Combine(Path.GetTempPath(), "update_" + Path.GetFileName(msixUrl));
            await _fileDownloadService.DownloadFileAsync(msixUrl, destinationPath);

            string appName = "FDW.VKM";
            string command;

            if (forceInstall)
            {
                OnInstallStatusChanged("Принудительная установка пакета...");
                command = GetForceInstallCommand(destinationPath, appName);
            }
            else
            {
                command = _systemService.IsAppInstalled("AppInstaller") ?
                    GetAppInstallerCommand(destinationPath) :
                    GetPowerShellInstallCommand(destinationPath, appName);
            }

            string output = ExecutePowerShellCommand(command);
            OnInstallStatusChanged($"Ответ PowerShell:\r\n{output}");

            if (output.Contains("false"))
            {
                OnInstallStatusChanged($"Ошибка: {appName} не найден после установки!");
            }

            OnInstallStatusChanged("Обновление завершено!");
        }

        public void InstallLatestDotNetRuntime()
        {
            var version = new Version(RuntimeInformation.FrameworkDescription.Replace(".NET ", ""));
            ExecutePowerShellCommand($"echo Y | winget install Microsoft.DotNet.DesktopRuntime.{version.Major}");
        }

        private string GetForceInstallCommand(string path, string appName)
        {
            return $"Add-AppxPackage -Path \"{path}\" -ForceUpdateFromAnyVersion; " +
                   $"if ((Get-AppxPackage).Name -like '*{appName}*') {{ " +
                   $"$pkg = (Get-AppxPackage -Name *{appName}*).PackageFamilyName; " +
                   $"Start-Process \"explorer.exe\" -ArgumentList \"shell:AppsFolder\\$pkg!App\" }} else {{ Write-Output \"false\" }}";
        }

        private string GetAppInstallerCommand(string path)
        {
            return $"& \"{path}\"";
        }

        private string GetPowerShellInstallCommand(string path, string appName)
        {
            return $"Add-AppxPackage -Path \"{path}\"; " +
                   $"if ((Get-AppxPackage).Name -like '*{appName}*') {{ " +
                   $"$pkg = (Get-AppxPackage -Name *{appName}*).PackageFamilyName; " +
                   $"Start-Process \"explorer.exe\" -ArgumentList \"shell:AppsFolder\\$pkg!App\" }} else {{ Write-Output \"false\" }}";
        }

        private string ExecutePowerShellCommand(string command)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"$OutputEncoding = [Console]::OutputEncoding = [Text.Encoding]::UTF8; {command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        protected virtual void OnInstallStatusChanged(string status)
        {
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = status });
        }
    }
}
