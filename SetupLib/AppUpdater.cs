using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace SetupLib
{
    public class AppUpdater
    {
        public string currentVersion;
        private readonly Octokit.GitHubClient client;

        public AppUpdater(string currentVersion)
        {
            client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("VKUI3"));
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

        public string version;
        public string Name { get; private set; }
        public string Tit { get; private set; }
        public string date { get; private set; }
        public int sizeFile;
        public string UriDownload { get; private set; }
        public string UriDownloadMSIX { get; private set; }

        public async Task<bool> CheckForUpdates()
        {
            var releases = await client.Repository.Release.GetAll("MaKrotos", "VKUI3");

            foreach (var release in releases)
            {
                if (string.Compare(release.TagName, currentVersion) <= 0)
                {
                    Console.WriteLine("Версия вашего приложения не ниже, чем последняя версия.");
                    return false;
                }

                string osArchitecture = GetOSArchitecture();


                var msixAsset = release.Assets.FirstOrDefault(asset => asset.Name.Contains(osArchitecture) && asset.Name.EndsWith(".msix"))
                                      ?? release.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".msixbundle")) ?? null;


                if (msixAsset != null)
                {
                    Console.WriteLine($"Доступна новая версия: {release.TagName}");

                    this.version = release.TagName;
                    this.Name = release.Name;
                    this.Tit = release.Body.ToString();

                    var cerAsset = release.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".cer")) ?? null;

                    if (cerAsset == null)
                    {
                        // Если сертификат не найден, ищем его в более старых релизах
                        foreach (var oldRelease in releases.Where(r => string.Compare(r.TagName, release.TagName) < 0))
                        {
                            cerAsset = oldRelease.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".cer")) ?? null;
                            if (cerAsset != null)
                            {
                                break;
                            }
                        }
                    }

                    if (cerAsset != null)
                    {
                        this.sizeFile = msixAsset.Size;
                        this.UriDownload = cerAsset.BrowserDownloadUrl;
                        this.UriDownloadMSIX = msixAsset.BrowserDownloadUrl;
                        return true;
                    }
                }
            }

            Console.WriteLine("В папке релиза нет файла msixAsset.");
            return false;
        }



        public async Task DownloadAndOpenFile()
        {
            var httpClient = new HttpClient();

            // Проверяем, запущено ли приложение с правами администратора.
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            if (isElevated)
            {
            

                // Сначала скачиваем и устанавливаем сертификат
                using (var response = await httpClient.GetAsync(UriDownload, HttpCompletionOption.ResponseHeadersRead))
                using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    var path = Path.Combine(Path.GetTempPath(), Path.GetFileName(UriDownload));
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

                                // Вызовите событие

                                /*
                                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs
                                {
                                    TotalBytes = response.Content.Headers.ContentLength.Value,
                                    BytesDownloaded = totalRead,
                                    Percentage = percentage
                                });
                                */
                            }
                        } while (isMoreToRead);
                    }

                    // Установка сертификата
                    X509Certificate2 cert = new X509Certificate2(path);
                    X509Store store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadWrite);

                    // Проверка на существование сертификата и его срок действия
                    bool certificateExists = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false).Count > 0;
                    if (!certificateExists || cert.NotAfter <= DateTime.Now)
                    {
                        store.Add(cert);
                    }

                    store.Close();
                }
            }

            bool isInstalled = IsAppInstalled("AppInstaller");

            if (!isInstalled)
            if (!IsAppInstalled("WindowsAppRuntime"))
            {
                // Затем скачиваем файл .msix
                using (var response = await httpClient.GetAsync(GetOSArchitectureURI()))
                using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    var path = Path.Combine(Path.GetTempPath(), Path.GetFileName(UriDownloadMSIX));
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

                                // Вызовите событие
                                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs
                                {
                                    TotalBytes = response.Content.Headers.ContentLength.Value,
                                    BytesDownloaded = totalRead,
                                    Percentage = percentage
                                });
                            }
                        } while (isMoreToRead);
                    }




                    string command = $"Add-AppxPackage -Path {path}";
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"{command}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    Process process = new Process() { StartInfo = startInfo };
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                }
            }


            // Затем скачиваем файл .msix
            using (var response = await httpClient.GetAsync(UriDownloadMSIX))
            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                var path = Path.Combine(Path.GetTempPath(), Path.GetFileName(UriDownloadMSIX));
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

                            // Вызовите событие
                            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs
                            {
                                TotalBytes = response.Content.Headers.ContentLength.Value,
                                BytesDownloaded = totalRead,
                                Percentage = percentage
                            });
                        }
                    } while (isMoreToRead);
                }


         
                if (isInstalled)
                {

                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = "powershell.exe";
                    startInfo.Arguments = "-NoProfile -ExecutionPolicy unrestricted -Command &{" + path + "}";
                    process.StartInfo = startInfo;
                    process.Start();

                }
                else
                {

                    string appName = "FDW.VKM";
                    string command = $"Add-AppxPackage -Path {path}; if ((Get-AppxPackage).Name -like '*{appName}*') {{ $pkg = (Get-AppxPackage -Name *{appName}*).PackageFamilyName; Start-Process \"explorer.exe\" -ArgumentList \"shell:AppsFolder\\$pkg!App\" }} else {{ Write-Output \"false\" }}";
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"{command}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    Process process = new Process() { StartInfo = startInfo };
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (output.Contains("false"))
                    {
                        Console.WriteLine($"{appName} не найден");
                    }


                }
            }
        }




        static bool IsAppInstalled(string appName)
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
            Process process = new Process() { StartInfo = startInfo };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
       
            return output.Contains("True");
        }


        public static string GetOSArchitecture()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                return "ARM64";
            }

            else if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                return "x64";
            }
            else if (RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.X86)
            {
                return "x86";
            }
            else
            {
                throw new Exception("Неизвестная архитектура");
            }
        }


        public static Uri GetOSArchitectureURI()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                return new Uri("https://github.com/MaKrotos/VKUI3/releases/download/0.1.0.0/Microsoft.WindowsAppRuntimeARM.1.4.msix");
            }
            
            else if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                return new Uri("https://github.com/MaKrotos/VKUI3/releases/download/0.1.0.0/Microsoft.WindowsAppRuntimeX64.1.4.msix");
            }
            else if (RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.X86)
            {
                return new Uri("https://github.com/MaKrotos/VKUI3/releases/download/0.1.0.0/Microsoft.WindowsAppRuntimeX86.1.4.msix");
            }
            else
            {
                throw new Exception("Неизвестная архитектура");
            }
        }



      



    }
}
