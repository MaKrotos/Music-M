using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Automation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Upload
{
    public sealed partial class UploadController : UserControl
    {
        public UploadController()
        {
            this.InitializeComponent();

            this.DataContextChanged += DownloadController_DataContextChanged;
            this.Loaded += DownloadController_Loaded;
            this.Unloaded += DownloadController_Unloaded;

        }

        private void DownloadController_Unloaded(object sender, RoutedEventArgs e)
        {
            if (playListDownload != null)
            {
                playListDownload.OnProgressChanged -= OnTrackDownloaded_Event;

            }
        }

        Helpers.Animations.AnimationsChangeFontIcon animationsChangeFontIcon = null;

        private void DownloadController_Loaded(object sender, RoutedEventArgs e)
        {

        }

        UploadTrack playListDownload { get; set; }
        private void DownloadController_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            if (playListDownload != null)
            {
                playListDownload.OnProgressChanged -= OnTrackDownloaded_Event;

            }
            playListDownload = (DataContext as UploadTrack);
            DownloadTitle.Text = playListDownload.name;

            if (DownloadTitle.Text.Length > 20)
            {
                DownloadTitle.Text = DownloadTitle.Text.Substring(0, 20) + "...";
            }


            playListDownload.OnProgressChanged += OnTrackDownloaded_Event;

            updateUI();
        }

        private void OnTrackDownloaded_Event(int obj)
        {
            updateUI();
        }



        private void updateUI()
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {


                    DownloadProgressBar.Value = playListDownload.percent;
                    // Объявляем об изменении значения прогресса для экранного диктора
                    var automationPeer = FrameworkElementAutomationPeer.FromElement(DownloadProgressBar);
                    if (automationPeer != null)
                    {
                        automationPeer.RaisePropertyChangedEvent( RangeValuePatternIdentifiers.ValueProperty, playListDownload.percent - 1, playListDownload.percent);
                    }

                }
                catch { }
            });
        }


        private void Cancel_clicked(object sender, RoutedEventArgs e)
        {
            playListDownload.CancelDownload();
        }


    }
}
