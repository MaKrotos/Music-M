using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Text;

namespace SetupLib
{
    public class AppUpdater
    {
        public string currentVersion;
        private readonly Octokit.GitHubClient client;

        public AppUpdater(string currentVersion)
        {
            client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("Music-M"));
            this.currentVersion = currentVersion;
        }

        // Определите делегат и событие
        public delegate void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e);
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        // Определите класс аргументов события
        public class DownloadProgressChangedEventArgs : EventArgs
        {
            public long TotalBytes { get; set; }
            public long BytesDownloaded { get; set; }
            public double Percentage { get; set; }
        }

        // Новый делегат и событие для статуса установки
        public delegate void InstallStatusChangedEventHandler(object sender, InstallStatusChangedEventArgs e);
        public event InstallStatusChangedEventHandler InstallStatusChanged;

        public class InstallStatusChangedEventArgs : EventArgs
        {
            public string Status { get; set; }
        }

        public string version;
        public string Name { get; private set; }
        public string Tit { get; private set; }
        public string date { get; private set; }
        public int sizeFile;
        public string UriDownload { get; private set; }
        public string UriDownloadMSIX { get; private set; }

        public async Task<bool> CheckForUpdates()
        {
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Проверка наличия обновлений..." });
            var releases = await client.Repository.Release.GetAll("MaKrotos", "Music-M");

            foreach (var release in releases)
            {
                if (string.Compare(release.TagName, currentVersion) <= 0)
                {
                    InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Установлена последняя версия: {release.TagName}" });
                    Console.WriteLine("Версия вашего приложения не ниже, чем последняя версия.");
                    return false;
                }

                string osArchitecture = GetOSArchitecture();
                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Найдена новая версия: {release.TagName} ({osArchitecture})" });

                var msixAsset = release.Assets.FirstOrDefault(asset => asset.Name.Contains(osArchitecture) && asset.Name.EndsWith(".msix"))
                                      ?? release.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".msixbundle")) ?? null;

                if (msixAsset != null)
                {
                    this.version = release.TagName;
                    this.Name = release.Name;
                    this.Tit = release.Body.ToString();

                    var cerAsset = release.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".cer")) ?? null;

                    if (cerAsset == null)
                    {
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Сертификат не найден в этом релизе, ищем в предыдущих..." });
                        foreach (var oldRelease in releases.Where(r => string.Compare(r.TagName, release.TagName) < 0))
                        {
                            cerAsset = oldRelease.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".cer")) ?? null;
                            if (cerAsset != null)
                            {
                                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Сертификат найден в релизе {oldRelease.TagName}" });
                                break;
                            }
                        }
                    }

                    if (cerAsset != null)
                    {
                        this.sizeFile = msixAsset.Size;
                        this.UriDownload = cerAsset.BrowserDownloadUrl;
                        this.UriDownloadMSIX = msixAsset.BrowserDownloadUrl;
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Информация о релизе получена успешно." });
                        return true;
                    }
                    else
                    {
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Сертификат не найден ни в одном релизе!" });
                    }
                }
            }

            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "В папке релиза нет файла msixAsset." });
            return false;
        }



        public async Task DownloadAndOpenFile(bool skip = false, bool forceInstall = false)
        {
            try
            {
                if (!skip)
                {
                    InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Проверка прав администратора..." });
                    bool isElevated;
                    using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                    {
                        WindowsPrincipal principal = new WindowsPrincipal(identity);
                        isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                    }

                    if (isElevated)
                    {
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Скачивание и установка сертификата..." });
                        await intsallCertAsync();
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Сертификат установлен." });
                    }

                    InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Проверка наличия AppInstaller..." });
                    bool isAppInstallerInstalled = IsAppInstalled("AppInstaller");
                    if (!isAppInstallerInstalled)
                    {
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Скачивание и установка AppInstaller..." });
                        await InstallAppInstallerAsync();
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "AppInstaller установлен." });
                    }
                    
                    InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Проверка наличия WindowsAppRuntime..." });
                    if (!IsAppInstalled("WindowsAppRuntime.1.6"))
                    {
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Скачивание и установка WindowsAppRuntime..." });
                        await DownlloadAppRuntimeAsync();
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "WindowsAppRuntime установлен." });
                    }
                }

                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Скачивание и установка обновления..." });
                await downloadUpdateAsync(forceInstall);
                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Обновление завершено!" });
            }
            catch (Exception ex)
            {
                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Ошибка: {ex.Message}" });
                throw;
            }
        }

        private async Task downloadUpdateAsync(bool forceInstall = false)
        {
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Скачивание файла обновления..." });
            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) })
            using (var response = await httpClient.GetAsync(UriDownloadMSIX))
            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                var path = Path.Combine(Path.GetTempPath(), "update_" + Path.GetFileName(UriDownloadMSIX));
                using (var streamToWriteTo = File.Create(path))
                {
                    var totalRead = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await streamToWriteTo.WriteAsync(buffer, 0, read);

                            totalRead += read;
                            var percentage = totalRead * 100d / response.Content.Headers.ContentLength.Value;
                            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs
                            {
                                TotalBytes = response.Content.Headers.ContentLength.Value,
                                BytesDownloaded = totalRead,
                                Percentage = percentage
                            });
                        }
                    } while (isMoreToRead);
                    await streamToWriteTo.FlushAsync();
                }

                string appName = "FDW.VKM";
                string command;
                
                if (forceInstall)
                {
                    InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Принудительная установка пакета..." });
                    command = $"Add-AppxPackage -Path {path} -ForceUpdateFromAnyVersion; if ((Get-AppxPackage).Name -like '*{appName}*') {{ $pkg = (Get-AppxPackage -Name *{appName}*).PackageFamilyName; Start-Process \"explorer.exe\" -ArgumentList \"shell:AppsFolder\\$pkg!App\" }} else {{ Write-Output \"false\" }}";
                }
                else
                {
                    bool isAppInstallerInstalled = IsAppInstalled("AppInstaller");
                    
                    if (isAppInstallerInstalled)
                    {
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Запуск AppInstaller..." });
                        command = $"& {path}";
                    }
                    else
                    {
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Установка пакета через PowerShell..." });
                        command = $"Add-AppxPackage -Path {path}; if ((Get-AppxPackage).Name -like '*{appName}*') {{ $pkg = (Get-AppxPackage -Name *{appName}*).PackageFamilyName; Start-Process \"explorer.exe\" -ArgumentList \"shell:AppsFolder\\$pkg!App\" }} else {{ Write-Output \"false\" }}";
                    }
                }
                
                ProcessStartInfo startInfo = new ProcessStartInfo();
                Process process = new Process();
                
                if (forceInstall || !IsAppInstalled("AppInstaller"))
                {
                    InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Выполнение PowerShell-установки..." });
                    startInfo.FileName = "powershell.exe";
                    startInfo.Arguments = $"-Command \"{command}\"";
                    startInfo.RedirectStandardOutput = true;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.StandardOutputEncoding = Encoding.Unicode;
                    
                    process.StartInfo = startInfo;
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Ответ PowerShell:\r\n{output}" });

                    if (output.Contains("false"))
                    {
                        InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Ошибка: {appName} не найден после установки!" });
                        Console.WriteLine($"{appName} не найден");
                    }
                }
                else
                {
                    InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Запуск установки через AppInstaller..." });
                    startInfo.FileName = "powershell.exe";
                    startInfo.Arguments = $"-NoProfile -ExecutionPolicy unrestricted -Command \"{command}\"";
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.StandardOutputEncoding = Encoding.Unicode;
                    
                    process.StartInfo = startInfo;
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Ответ PowerShell (AppInstaller):\r\n{output}" });
                }
            }
        }

        private async Task DownlloadAppRuntimeAsync()
        {
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Скачивание WindowsAppRuntime..." });
            var runtimeUri = GetOSArchitectureURI();
            var runtimeInstallerPath = Path.Combine(Path.GetTempPath(), "runtime_" + Path.GetFileName(runtimeUri.ToString()));
            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) })
            using (var response = await httpClient.GetAsync(runtimeUri))
            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                using (var streamToWriteTo = File.Create(runtimeInstallerPath))
                {
                    var totalRead = 0L;
                    var buffer = new byte[8192];

                    while (true)
                    {
                        var read = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0) break;

                        await streamToWriteTo.WriteAsync(buffer, 0, read);

                        totalRead += read;
                        var percentage = totalRead * 100d / response.Content.Headers.ContentLength.Value;

                        DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs
                        {
                            TotalBytes = response.Content.Headers.ContentLength.Value,
                            BytesDownloaded = totalRead,
                            Percentage = percentage
                        });
                    }
                    // Явно сбрасываем и закрываем поток
                    await streamToWriteTo.FlushAsync();
                }
            }
            // Только после using запускаем процесс установки!
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Установка WindowsAppRuntime..." });
            string command = $"Add-AppxPackage -Path \"{runtimeInstallerPath}\"";
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.Unicode
            };
            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Ответ PowerShell (WindowsAppRuntime):\r\n{output}" });
            }
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "WindowsAppRuntime установлен." });
        }

        private async Task intsallCertAsync()
        {
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Скачивание сертификата..." });
            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) })
            using (var response = await httpClient.GetAsync(UriDownload, HttpCompletionOption.ResponseHeadersRead))
            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                var path = Path.Combine(Path.GetTempPath(), "cert_" + Path.GetFileName(UriDownload));
                using (var streamToWriteTo = File.Create(path))
                {
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                }

                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Установка сертификата..." });
                X509Certificate2 cert = new X509Certificate2(path);
                X509Store store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                bool certificateExists = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false).Count > 0;
                if (!certificateExists || cert.NotAfter <= DateTime.Now)
                {
                    store.Add(cert);
                    InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Сертификат добавлен в хранилище." });
                }
                else
                {
                    InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Сертификат уже существует и действителен." });
                }
                store.Close();
            }
        }



        public bool IsVersionInstalled(string targetVersion)
        {
            try
            {
                var installedVersions = GetInstalledDotNetVersions();
                var targetVersionNumber = GetVersionNumber(targetVersion);


                foreach (var version in installedVersions)
                {
                    var ver = GetVersionNumber(version);
                    if (ver.Major != targetVersionNumber.Major) continue;
                    var versionNumber = GetVersionNumber(version);
                    if (versionNumber != null && versionNumber >= targetVersionNumber && version.Contains("WindowsDesktop"))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex) { return false; };
        }

        private Version GetVersionNumber(string versionString)
        {
            var match = Regex.Match(versionString, @"(\d+(\.\d+)+)");
            if (match.Success)
            {
                return new Version(match.Value);
            }

            return null;
        }


        public List<string> GetInstalledDotNetVersions()
        {
            var versions = new List<string>();

            //  .NET SDK
            versions.AddRange(GetDotNetInfo("dotnet --list-sdks"));

            //  .NET Runtime
            versions.AddRange(GetDotNetInfo("dotnet --list-runtimes"));

            // Удалить пустые строки и объединить все версии в один список
            versions = versions.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();

            return versions;
        }

        private List<string> GetDotNetInfo(string command)
        {
            var info = new List<string>();
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                info.Add(line);
            }

            return info;
        }




        bool IsAppInstalled(string appName)
        {
            string command = $"if ((Get-AppxPackage).Name -like '*{appName}*') {{ Write-Output \"True\" }} else {{ Write-Output \"False\" }}";
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.StandardOutputEncoding = Encoding.Unicode;
            Process process = new Process() { StartInfo = startInfo };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();

            return output.Contains("True");
        }

        public async Task InstallAppInstallerAsync()
        {
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Скачивание AppInstaller..." });
            var uri = "https://github.com/microsoft/winget-cli/releases/download/v1.7.10661/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle";
            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) })
            using (var response = await httpClient.GetAsync(uri))
            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                var path = Path.Combine(Path.GetTempPath(), "appinstaller_" + Path.GetFileName(uri));
                using (var streamToWriteTo = File.Create(path))
                {
                    var totalRead = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await streamToWriteTo.WriteAsync(buffer, 0, read);

                            totalRead += read;
                            var percentage = totalRead * 100d / response.Content.Headers.ContentLength.Value;
                            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs
                            {
                                TotalBytes = response.Content.Headers.ContentLength.Value,
                                BytesDownloaded = totalRead,
                                Percentage = percentage
                            });
                        }
                    } while (isMoreToRead);
                    await streamToWriteTo.FlushAsync();
                }
                // Только после выхода из using запускаем процесс установки
                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "Установка AppInstaller..." });
                string command = $"Add-AppxPackage -Path {path};";
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                startInfo.StandardOutputEncoding = Encoding.Unicode;
                Process process = new Process() { StartInfo = startInfo };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = $"Ответ PowerShell (AppInstaller):\r\n{output}" });
            }
            InstallStatusChanged?.Invoke(this, new InstallStatusChangedEventArgs { Status = "AppInstaller установлен." });
        }




        public void InstallLatestDotNetAppRuntime()
        {
            var vers = GetVersionNumber(RuntimeInformation.FrameworkDescription);
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command echo Y | winget install Microsoft.DotNet.DesktopRuntime.{vers.Major}",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
        }



        public string GetOSArchitecture()
        {
            if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                return "ARM64";
            }

            else if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                return "x64";
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.X86)
            {
                return "x86";
            }
            else
            {
                throw new Exception("Неизвестная архитектура");
            }
        }

        public bool CheckIfWingetIsInstalled()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.Arguments = "-Command \"& {winget; echo $?}\"";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            // Если последняя строка равна "True", значит winget установлен
            return output.Trim().EndsWith("True");
        }


        public Uri GetOSArchitectureURI()
        {
            if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                return new Uri("https://github.com/MaKrotos/Music-M/releases/download/0.1.0.0/WindowsAppRuntimeInstall.1.6.3-arm64.exe");
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                return new Uri("https://github.com/MaKrotos/Music-M/releases/download/0.1.0.0/WindowsAppRuntimeInstall.1.6.3-x64.exe");
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.X86)
            {
                return new Uri("https://github.com/MaKrotos/Music-M/releases/download/0.1.0.0/WindowsAppRuntimeInstall.1.6.3-x86.exe");
            }
            else
            {
                throw new Exception("Неизвестная архитектура");
            }
        }

    }

}
