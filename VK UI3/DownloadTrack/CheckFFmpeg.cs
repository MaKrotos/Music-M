using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.DownloadTrack
{
    public class CheckFFmpeg
    {
        public string? getLinkFFMPEG()
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                    return "https://github.com/MaKrotos/Music-M/releases/download/0.1.0.0/Microsoft.WindowsAppRuntimeX64.1.4.msix";
                case Architecture.X64:
                    return "https://github.com/MaKrotos/Music-M/releases/download/0.1.0.0/Microsoft.WindowsAppRuntimeX86.1.4.msix";
                default:
                    return null;
            }
        }
    }

    public class DownloadFileWithProgress
    {
        public delegate void DownloadCompletedHandler(object sender, EventArgs e);
        public event DownloadCompletedHandler DownloadCompleted;

        public delegate void ProgressChangedHandler(object sender, DownloadProgressChangedEventArgs e);
        public event ProgressChangedHandler ProgressChanged;

        private WebClient webClient;
        private CheckFFmpeg checkFFmpeg = new CheckFFmpeg();

        public DownloadFileWithProgress(string destination)
        {
            webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChangedEvent);
            this.DownloadFile(destination);
        }

        public void DownloadFile(string destination)
        {
            string? url = checkFFmpeg.getLinkFFMPEG();
            if (url != null)
            {
                webClient.DownloadFileAsync(new Uri(url), destination);
            }
        }

        public void CancelDownload()
        {
            webClient.CancelAsync();
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            DownloadCompleted?.Invoke(this, e);
        }

        private void ProgressChangedEvent(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }
    }
}
