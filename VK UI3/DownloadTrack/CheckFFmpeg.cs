using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VK_UI3.DB;

namespace VK_UI3.DownloadTrack
{
    public class CheckFFmpeg
    {
        public async Task<double?> getFileSizeInMBAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(getLinkFFMPEG(), HttpCompletionOption.ResponseHeadersRead);
                    if (response.IsSuccessStatusCode)
                    {
                        var contentLength = response.Content.Headers.ContentLength;
                        return contentLength.HasValue ? (double)contentLength.Value / (1024 * 1024) : (double?)null;
                    }
                }
            }
            catch
            {
                // Log error if needed
            }
            return null;
        }

        public string GetFFmpegDirectory()
        {
            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                path = Path.Combine(path, "VKMMKZ", "FFmpeg");

                // Создаем все поддиректории, если они не существуют
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);

                    // Добавляем скрытый атрибут для родительской папки VKMMKZ (опционально)
                    var parentDir = Path.GetDirectoryName(path);
                    if (Directory.Exists(parentDir))
                    {
                        try
                        {
                            File.SetAttributes(parentDir, File.GetAttributes(parentDir) | FileAttributes.Hidden);
                        }
                        catch
                        {
                            // Если не получилось установить атрибут - игнорируем
                        }
                    }
                }

                return path;
            }
            catch (Exception ex)
            {
                // Логируем ошибку, если нужно
                Console.WriteLine($"Ошибка при создании директории FFmpeg: {ex.Message}");
                throw; // или возвращаем путь к временной директории
            }
        }

        public string GetFFmpegPath()
        {
            return Path.Combine(GetFFmpegDirectory(), RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg");
        }

        public bool IsExist()
        {
            string directory = GetFFmpegDirectory();

            if (!Directory.Exists(directory))
            {
                return false;
            }

            string[] requiredFiles = new string[]
            {
                "avcodec-61.dll",
                "avdevice-61.dll",
                "avfilter-10.dll",
                "avformat-61.dll",
                "avutil-59.dll",
                "swresample-5.dll"
            };

            foreach (string file in requiredFiles)
            {
                if (!File.Exists(Path.Combine(directory, file)))
                {
                    return false;
                }
            }

            return true;
        }

        public string? getLinkFFMPEG()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {

                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X86:
                        return "https://github.com/MaKrotos/Music-M/releases/download/0.3.2.5/ffmpeg-X32.zip";
                    case Architecture.X64:
                        return "https://github.com/MaKrotos/Music-M/releases/download/0.3.2.5/ffmpeg-X64.zip";
                    case Architecture.Arm64:
                        return "https://github.com/MaKrotos/Music-M/releases/download/0.3.2.5/ffmpeg-ARM64.zip";

                default:
                    return null;
            }
            // Add other platform support if needed
            return null;
        }
    }

    public class DownloadFileWithProgress
    {
        public bool isNowDownload = false;
        public delegate void DownloadCompletedHandler(object sender, EventArgs e);
        public event DownloadCompletedHandler DownloadCompleted;

        public delegate void ProgressChangedHandler(object sender, DownloadProgressChangedEventArgs e);
        public event ProgressChangedHandler ProgressChanged;

        private WebClient webClient;
        private CheckFFmpeg checkFFmpeg = new CheckFFmpeg();
        private static bool isDownloading = false;
        private static object downloadLock = new object();

        public DownloadFileWithProgress()
        {
            if (MainWindow.downloadFileWithProgress == null)
            {
                MainWindow.downloadFileWithProgress = this;

                webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChangedEvent);

                lock (downloadLock)
                {
                    if (!checkFFmpeg.IsExist() && !isDownloading)
                    {
                        MainWindow.mainWindow.requstDownloadFFMpegAsync();
                    }
                    else if (isDownloading)
                    {
                        MainWindow.mainWindow.MainWindow_showDownload();
                    }
                }
            }
            else
            {
                MainWindow.mainWindow.MainWindow_showDownload();
            }
        }

        public double? mb;

        public async void DownloadFile()
        {
            lock (downloadLock)
            {
                if (isDownloading)
                    return;
                isDownloading = true;
            }

            await Task.Run(async () =>
            {
                string tempZipPath = Path.Combine(Path.GetTempPath(), "ffmpeg_temp.zip");
                string? url = checkFFmpeg.getLinkFFMPEG();

                if (url != null)
                {
                    if (File.Exists(tempZipPath))
                    {
                        File.Delete(tempZipPath);
                    }

                    mb = await checkFFmpeg.getFileSizeInMBAsync();
                    webClient.DownloadFileAsync(new Uri(url), tempZipPath);
                }
            });
        }

        public void CancelDownload()
        {
            webClient.CancelAsync();
            foreach (var item in PlayListDownload.PlayListDownloads)
            {
                item.Cancel();
            }
            lock (downloadLock)
            {
                isDownloading = false;
            }
            MainWindow.downloadFileWithProgress = null;
        }

        private void ExtractFFmpegFromZip(string zipPath)
        {
            string extractPath = checkFFmpeg.GetFFmpegDirectory();
            string ffmpegBinaryName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";

            // Clear existing directory
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }
            Directory.CreateDirectory(extractPath);

            // Extract archive
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            // On Windows, the binary is in the 'bin' subdirectory
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string binPath = Path.Combine(extractPath, "bin", ffmpegBinaryName);
                if (File.Exists(binPath))
                {
                    string destinationPath = Path.Combine(extractPath, ffmpegBinaryName);
                    File.Move(binPath, destinationPath);
                }
            }

            // Clean up
            File.Delete(zipPath);
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            lock (downloadLock)
            {
                isDownloading = false;
            }

            if (e.Error != null)
            {
                // Handle download error
                DownloadCompleted?.Invoke(this, e);
                return;
            }

            try
            {
                string tempZipPath = Path.Combine(Path.GetTempPath(), "ffmpeg_temp.zip");
                if (File.Exists(tempZipPath))
                {
                    ExtractFFmpegFromZip(tempZipPath);
                }

                DownloadCompleted?.Invoke(this, EventArgs.Empty);

                if (SettingsTable.GetSetting("downloadALL") == null)
                    PlayListDownload.ResumeOnlyFirst();
                else
                    PlayListDownload.ResumeAll();

                MainWindow.mainWindow.MainWindow_showDownload();
            }
            catch (Exception ex)
            {
                // Handle extraction error
                DownloadCompleted?.Invoke(this, new AsyncCompletedEventArgs(ex, false, null));
            }
            finally
            {
                MainWindow.downloadFileWithProgress = null;
            }
        }

        private void ProgressChangedEvent(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }
    }
}