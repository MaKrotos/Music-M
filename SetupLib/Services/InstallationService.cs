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

        public async Task InstallUpdateAsync(AppUpdater appUpdater, bool skipCertCheck, bool forceInstall, string? PathInstallZIP = null)
        {
            if (!skipCertCheck)
            {
                await HandleCertificateInstallation(appUpdater._currentReleaseInfo.CertificateUrl);

                if (appUpdater.SelectedPackageType != PackageType.ZIP)
                await HandleDependenciesInstallation(forceInstall);
            }
            
            await InstallApplicationAsync(appUpdater.UriDownloadMSIX, appUpdater.SelectedPackageType, forceInstall, PathInstallZIP);
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

        private async Task InstallApplicationAsync(string packageUrl, PackageType packageType, bool forceInstall, string? PathInstallZIP = null)
        {
            OnInstallStatusChanged("Скачивание и установка обновления...");

            var destinationPath = Path.Combine(Path.GetTempPath(),
                $"update_{packageType}_{Path.GetFileName(packageUrl)}");

            await _fileDownloadService.DownloadFileAsync(packageUrl, destinationPath);

            // Проверяем, что файл действительно скачался
            if (!File.Exists(destinationPath))
            {
                throw new FileNotFoundException($"Файл не был скачан: {destinationPath}");
            }



            string command;

            if (packageType == PackageType.MSIX)
            {
                command = GetMsixInstallCommand(destinationPath, forceInstall);
            }
            else // PackageType.ZIP
            {
                // Определяем путь к Program Files на системном диске
                string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string extractPath = Path.Combine(programFilesPath, "VK M");
                command = GetZipInstallCommand(destinationPath, extractPath);
            }

            try
            {
                string output = ExecutePowerShellCommand(command);
                OnInstallStatusChanged($"Результат установки:\r\n{output}");
            }
            catch (Exception ex)
            {
                OnInstallStatusChanged($"Ошибка при установке: {ex.Message}");
                throw;
            }
            finally
            {
                // Очищаем временный файл
                try
                {  }
                catch { }
            }

            OnInstallStatusChanged("Обновление завершено!");
        }

        private string GetMsixInstallCommand(string path, bool forceInstall)
        {
            string appName = "FDW.VKM";

            // Базовая команда установки
            string installCommand = forceInstall ?
                $"Add-AppxPackage -Path \"{path}\" -ForceUpdateFromAnyVersion -ErrorAction Stop" :
                $"Add-AppxPackage -Path \"{path}\" -ErrorAction Stop";

            // Для обычной установки проверяем AppInstaller
            if (!forceInstall && _systemService.IsAppInstalled("AppInstaller"))
            {
                return $"Start-Process \"{path}\" -Wait";
            }

            return installCommand +
                   " ; " +
                   "$installed = Get-AppxPackage -Name *" + appName + "* " +
                   " ; " +
                   "if ($installed) { " +
                   "    $pkg = $installed.PackageFamilyName " +
                   "    Write-Output \"Установка успешна. Запуск приложения...\" " +
                   "    Start-Sleep -Seconds 2 " +
                   "    Start-Process \"explorer.exe\" \"shell:AppsFolder\\$pkg!App\" " +
                   "} else { " +
                   "    Write-Error \"Приложение не было установлено\" " +
                   "    exit 1 " +
                   "}";
        }


        private string GetZipInstallCommand(string zipPath, string extractPath)
        {
            // Экранируем пути для PowerShell
            string escapedZipPath = zipPath.Replace("'", "''").Replace("\"", "\\\"");
            string escapedExtractPath = extractPath.Replace("'", "''").Replace("\"", "\\\"");

            // Используем StringBuilder для построения команды
            var sb = new StringBuilder();

            // Убираем проверку прав администратора из скрипта - это должно быть на уровне запуска
            sb.AppendLine("Write-Output \"Запуск процесса установки...\"");
            sb.AppendLine("");

            sb.AppendLine("# Закрытие процессов VK M перед установкой");
            sb.AppendLine("$processNames = @('VK M', 'VK_UI3')");
            sb.AppendLine("$processes = Get-Process -Name $processNames -ErrorAction SilentlyContinue");
            sb.AppendLine("if ($processes) {");
            sb.AppendLine("    Write-Output \"Завершение процессов: $($processes.Name -join ', ')\"");
            sb.AppendLine("    $processes | Stop-Process -Force");
            sb.AppendLine("    Start-Sleep -Seconds 3");
            sb.AppendLine("    # Проверяем, что процессы закрылись");
            sb.AppendLine("    $runningProcesses = Get-Process -Name $processNames -ErrorAction SilentlyContinue");
            sb.AppendLine("    if ($runningProcesses) {");
            sb.AppendLine("        Write-Output \"Предупреждение: некоторые процессы все еще работают\"");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine("");

            sb.AppendLine("# Определяем целевую директорию для установки");
            sb.AppendLine("if ([string]::IsNullOrEmpty('" + escapedExtractPath + "')) {");
            sb.AppendLine("    # Если extractPath не указан, используем текущую директорию скрипта");
            sb.AppendLine("    $targetDir = $PSScriptRoot");
            sb.AppendLine("    if (-not $targetDir) { $targetDir = Get-Location | Select-Object -ExpandProperty Path }");
            sb.AppendLine("    Write-Output \"Целевая директория не указана, используем текущую: $targetDir\"");
            sb.AppendLine("} else {");
            sb.AppendLine("    # Используем указанный extractPath");
            sb.AppendLine("    $targetDir = '" + escapedExtractPath + "'");
            sb.AppendLine("    # Создаем директорию, если она не существует");
            sb.AppendLine("    if (-not (Test-Path $targetDir)) {");
            sb.AppendLine("        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null");
            sb.AppendLine("        Write-Output \"Создана целевая директория: $targetDir\"");
            sb.AppendLine("    }");
            sb.AppendLine("    Write-Output \"Установка в указанную директорию: $targetDir\"");
            sb.AppendLine("}");
            sb.AppendLine("");

            sb.AppendLine("# Распаковка ZIP архива во временную директорию");
            sb.AppendLine("$tempExtractPath = Join-Path $env:TEMP \"VK_Update_Temp_$([System.Guid]::NewGuid().ToString('N').Substring(0, 8))\"");
            sb.AppendLine("if (Test-Path $tempExtractPath) {");
            sb.AppendLine("    Remove-Item $tempExtractPath -Recurse -Force");
            sb.AppendLine("}");
            sb.AppendLine("New-Item -ItemType Directory -Path $tempExtractPath -Force | Out-Null");
            sb.AppendLine("Write-Output \"Временная распаковка в: $tempExtractPath\"");
            sb.AppendLine($"Expand-Archive -Path '{escapedZipPath}' -DestinationPath $tempExtractPath -Force");
            sb.AppendLine("");

            sb.AppendLine("# Поиск папки win-x64");
            sb.AppendLine("$winX64Path = Get-ChildItem -Path $tempExtractPath -Recurse -Directory -Filter \"win-x64\" | Select-Object -First 1");
            sb.AppendLine("if (-not $winX64Path) {");
            sb.AppendLine("    # Если нет win-x64, используем корень распакованного архива");
            sb.AppendLine("    $winX64Path = Get-Item $tempExtractPath");
            sb.AppendLine("    Write-Output \"Папка win-x64 не найдена, используем корень архива\"");
            sb.AppendLine("}");
            sb.AppendLine("");

            sb.AppendLine("Write-Output \"Копирование файлов из: $($winX64Path.FullName)\"");
            sb.AppendLine("Write-Output \"В целевую директорию: $targetDir\"");
            sb.AppendLine("");

            sb.AppendLine("# Копируем файлы напрямую (без создания дополнительных процессов)");
            sb.AppendLine("try {");
            sb.AppendLine("    # Ждем немного перед копированием");
            sb.AppendLine("    Start-Sleep -Milliseconds 500");
            sb.AppendLine("    ");
            sb.AppendLine("    # Копируем файлы по одному с повторными попытками");
            sb.AppendLine("    $retryCount = 3");
            sb.AppendLine("    $retryDelay = 1000");
            sb.AppendLine("    ");
            sb.AppendLine("    Get-ChildItem -Path $($winX64Path.FullName) -Recurse | ForEach-Object {");
            sb.AppendLine("        $relativePath = $_.FullName.Substring($winX64Path.FullName.Length + 1)");
            sb.AppendLine("        $destinationFile = Join-Path $targetDir $relativePath");
            sb.AppendLine("        ");
            sb.AppendLine("        if ($_.PSIsContainer) {");
            sb.AppendLine("            # Создаем директорию");
            sb.AppendLine("            New-Item -ItemType Directory -Path $destinationFile -Force | Out-Null");
            sb.AppendLine("        } else {");
            sb.AppendLine("            # Копируем файл с повторными попытками");
            sb.AppendLine("            $attempt = 0");
            sb.AppendLine("            while ($attempt -lt $retryCount) {");
            sb.AppendLine("                try {");
            sb.AppendLine("                    Copy-Item $_.FullName $destinationFile -Force");
            sb.AppendLine("                    break");
            sb.AppendLine("                } catch {");
            sb.AppendLine("                    $attempt++");
            sb.AppendLine("                    if ($attempt -eq $retryCount) {");
            sb.AppendLine("                        Write-Warning \"Не удалось скопировать: $($_.Name)\"");
            sb.AppendLine("                    } else {");
            sb.AppendLine("                        Start-Sleep -Milliseconds $retryDelay");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("    ");
            sb.AppendLine("    Write-Output \"Копирование завершено\"");
            sb.AppendLine("} catch {");
            sb.AppendLine("    Write-Error \"Ошибка при копировании: $_\"");
            sb.AppendLine("}");
            sb.AppendLine("");

            sb.AppendLine("# Удаляем временные файлы");
            sb.AppendLine("Remove-Item $tempExtractPath -Recurse -Force -ErrorAction SilentlyContinue");
            sb.AppendLine("");

            sb.AppendLine("# Поиск исполняемого файла VK M.exe в целевой директории");
            sb.AppendLine("$exePath = Get-ChildItem -Path $targetDir -Recurse -Filter \"VK M.exe\" | Select-Object -First 1");
            sb.AppendLine("if (-not $exePath) {");
            sb.AppendLine("    # Если VK M.exe не найден, ищем любое другое .exe");
            sb.AppendLine("    $exePath = Get-ChildItem -Path $targetDir -Recurse -Filter \"*.exe\" | Select-Object -First 1");
            sb.AppendLine("}");
            sb.AppendLine("");

            sb.AppendLine("if ($exePath) {");
            sb.AppendLine("    # Запуск приложения");
            sb.AppendLine("    try {");
            sb.AppendLine("        Start-Sleep -Milliseconds 500");
            sb.AppendLine("        Start-Process -FilePath $exePath.FullName");
            sb.AppendLine("        Write-Output \"Приложение успешно запущено: $($exePath.Name)\"");
            sb.AppendLine("    } catch {");
            sb.AppendLine("        Write-Error \"Ошибка при запуске приложения: $_\"");
            sb.AppendLine("    }");
            sb.AppendLine("} else {");
            sb.AppendLine("    Write-Output \"Исполняемый файл не найден в архиве\"");
            sb.AppendLine("    Write-Output \"Содержимое директории:\"");
            sb.AppendLine("    Get-ChildItem -Path $targetDir -Recurse | ForEach-Object { $_.FullName }");
            sb.AppendLine("}");

            sb.AppendLine("Write-Output \"Обновление завершено!\"");
            sb.AppendLine("Read-Host \"Нажмите Enter для выхода\"");

            return sb.ToString();
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
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -Command \"{command.Replace("\"", "\\\"")}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    Verb = "runas",
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"PowerShell error (exit code {process.ExitCode}): {error}");
                    }

                    return output + (string.IsNullOrEmpty(error) ? "" : $"\nErrors: {error}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"PowerShell execution failed: {ex.Message}", ex);
            }
        }


        protected virtual void OnInstallStatusChanged(string status)
        {
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = status });
        }
    }
}
