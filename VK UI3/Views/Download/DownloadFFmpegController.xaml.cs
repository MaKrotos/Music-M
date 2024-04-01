using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using VK_UI3.DownloadTrack;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Download
{
    public sealed partial class DownloadFFmpegController : UserControl
    {
        public DownloadFFmpegController()
        {
            this.InitializeComponent();
     
            this.Loaded += DownloadFFmpegController_Loaded;
            this.Unloaded += DownloadFFmpegController_Unloaded;
        }

        private void DownloadFFmpegController_Unloaded(object sender, RoutedEventArgs e)
        {
            if (MainWindow.downloadFileWithProgress != null)
            {
                MainWindow.downloadFileWithProgress.ProgressChanged -= DownloadFileWithProgress_ProgressChanged;
                MainWindow.downloadFileWithProgress.DownloadCompleted -= DownloadFileWithProgress_DownloadCompleted;
            }
        }

        private void DownloadFileWithProgress_DownloadCompleted(object sender, EventArgs e)
        {
            (sender as DownloadFileWithProgress).DownloadCompleted -= DownloadFileWithProgress_DownloadCompleted;
            (sender as DownloadFileWithProgress).ProgressChanged -= DownloadFileWithProgress_ProgressChanged;

        }

        private void DownloadFFmpegController_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainWindow.downloadFileWithProgress != null)
            {
                MainWindow.downloadFileWithProgress.ProgressChanged += DownloadFileWithProgress_ProgressChanged;
                MainWindow.downloadFileWithProgress.DownloadCompleted += DownloadFileWithProgress_DownloadCompleted;
            }
        }

        private void DownloadFileWithProgress_ProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                int progressPercentage = e.ProgressPercentage; // Прогресс в процентах
                DownloadProgressBar.Value = progressPercentage;
                pathText.Text = $"Загружено: {Math.Round((double)e.BytesReceived / (1024 * 1024))} Мб  из  {Math.Round((double)(sender as DownloadFileWithProgress).mb)} Мб ";
            });
         }

    }
}
