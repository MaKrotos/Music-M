using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using vkPosterBot.DB;

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
              
            }
            return null;
        }

        public string getPathFfmpeg()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, "VKMMKZ");
            path = Path.Combine(path, "FFMPeg.exe");
            return path;
        }
        public bool isExist()
        {
            if (File.Exists(getPathFfmpeg()))
            {
                
                return true;
            }
            return false;
        }
        public string? getLinkFFMPEG()
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                    return "https://github.com/MaKrotos/Music-M/releases/download/0.1.0.0/ffmpegX32.exe";
                case Architecture.X64:
                    return "https://github.com/MaKrotos/Music-M/releases/download/0.1.0.0/ffmpegX64.exe";
                default:
                    return null;
            }
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

   
        public DownloadFileWithProgress()
        {
            if (MainWindow.downloadFileWithProgress == null)
            {
                MainWindow.downloadFileWithProgress = this;


                webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChangedEvent);
                if (!checkFFmpeg.isExist())
                {
                    MainWindow.mainWindow.requstDownloadFFMpegAsync();
                }
            }
            else {

                MainWindow.mainWindow.MainWindow_showDownload();
            }
        }

        public double? mb;

        public async void DownloadFile()
        {
             _=   Task.Run(async () =>
                {
                    string path = checkFFmpeg.getPathFfmpeg() + ".temp";
                    if (File.Exists(path)) { File.Delete(path); }

                    mb = await checkFFmpeg.getFileSizeInMBAsync();
                    string? url = checkFFmpeg.getLinkFFMPEG();
                    if (url != null)
                    {
                        webClient.OpenRead(url);


                        webClient.DownloadFileAsync(new Uri(url), path);
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
            MainWindow.downloadFileWithProgress = null;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
       
            string path = checkFFmpeg.getPathFfmpeg();
            string temp = path + ".temp";

            File.Move(temp, path);

            DownloadCompleted?.Invoke(this, e);
            MainWindow.downloadFileWithProgress = null;
            if (SettingsTable.GetSetting("downloadALL") == null)
                PlayListDownload.ResumeOnlyFirst();
            else
                PlayListDownload.ResumeAll();


            MainWindow.mainWindow.MainWindow_showDownload();
        }

        private void ProgressChangedEvent(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }
    }
}
