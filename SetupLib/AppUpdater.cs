using Microsoft.Win32;

using System.Diagnostics;

using System.Linq;
using System.Management;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

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
        public int sizeFile;
        public string UriDownload { get; private set; }
        public string UriDownloadMSIX { get; private set; }

        public async Task<bool> CheckForUpdates()
        {
            var releases = await client.Repository.Release.GetAll("MaKrotos", "VKUI3");
            var latestRelease = releases[0];

            if (latestRelease.TagName == currentVersion)
            {
                Console.WriteLine("Ваше приложение обновлено до последней версии.");
                return false;
            }

            if (string.Compare(latestRelease.TagName, currentVersion) < 0)
            {
                Console.WriteLine("Версия вашего приложения выше, чем последняя версия.");
                return false;
            }

            Console.WriteLine($"Доступна новая версия: {latestRelease.TagName}");

            this.version = latestRelease.TagName;
            this.Name = latestRelease.Name;
            this.Tit = latestRelease.Body.ToString();

            // Ищем файлы .cer и .msix в активах
            var cerAsset = latestRelease.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".cer"))?? null;
            var msixAsset = latestRelease.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".msix"));

            if (cerAsset == null)
            {
                foreach (var item in releases)
                {
                     cerAsset = latestRelease.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".cer")) ?? null;
                    if (cerAsset != null)
                    {
                        break;
                    }
                }
            }

            if (cerAsset != null && msixAsset != null)
            {
                this.sizeFile = msixAsset.Size;
                this.UriDownload = cerAsset.BrowserDownloadUrl;
                this.UriDownloadMSIX = msixAsset.BrowserDownloadUrl;
                return true;
            }

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
        }

        public static bool IsPackageInstalled(string packageName)
        {
            PackageManager packageManager = new PackageManager();
            IEnumerable<Package> packages = packageManager.FindPackagesForUser(string.Empty);
            foreach (var package in packages)
            {
                if (package.Id.FamilyName == packageName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
