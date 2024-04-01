using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using TagLib.Id3v2;
using VK_UI3.DownloadTrack;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Download
{
    public sealed partial class DownloadsList : UserControl
    {

        public ObservableCollection<PlayListDownload> PlayListDownloads { get { return PlayListDownload.PlayListDownloads; } }
        public DownloadsList()
        {
            this.InitializeComponent();
  
            this.Loading += DownloadsList_Loading;
            this.Unloaded += DownloadsList_Unloaded;
        }

        private void DownloadsList_Unloaded(object sender, RoutedEventArgs e)
        {
            TrackListView.Header = null;
        }

        private void DownloadsList_Loading(FrameworkElement sender, object args)
        {
            if (MainWindow.downloadFileWithProgress != null)
            {
                DownloadFFmpegController header = new DownloadFFmpegController();
                TrackListView.Header = header;
            }
        }


        
    }
}
