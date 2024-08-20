using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using VK_UI3.Controllers;
using VK_UI3.Helpers;
using VK_UI3.VKs.IVK;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayList : Page
    {
        //  public static List<ExtendedAudio> tracklist = new List<ExtendedAudio>();
        private IVKGetAudio Tracks
        {
            get
            {
                return AudioPlayer.iVKGetAudio;
            }
            set { AudioPlayer.iVKGetAudio = value; }

        }
        public PlayList()
        {
            this.InitializeComponent();
            this.Loaded += PlayListPage_Loaded;
            this.Unloaded += PlayListPage_Unloaded;
            AudioPlayer.onClickonTrack += AudioPlayer_onClickonTrack;
        }

        private void PlayListPage_Unloaded(object sender, RoutedEventArgs e)
        {
           // throw new NotImplementedException();
        }


        private void AudioPlayer_onClickonTrack(object sender, EventArgs e)
        {
          
            var audio = sender as ExtendedAudio;
            var ins = Tracks.listAudio.IndexOf(audio);

            if (ins < 0)
            {
                audio = Tracks.listAudio.FirstOrDefault(a => a.audio.Id == audio.audio.Id && a.audio.OwnerId == audio.audio.OwnerId);
                audio = Tracks.listAudio.FirstOrDefault(a => a.audio.Id == audio.audio.Id && a.audio.OwnerId == audio.audio.OwnerId);
                ins = Tracks.listAudio.IndexOf(audio);
            }

            if (ins >= 0 && ins < TrackListView.Items.Count)
            {
                var container = TrackListView.ContainerFromIndex(ins) as ListViewItem;
                if (container != null)
                {
                    var transform = container.TransformToVisual(TrackListView);
                    var position = transform.TransformPoint(new Point(0, 0));
                    double itemHeight = position.Y;

                    scrollViewer.ChangeView(null, scrollViewer.VerticalOffset + itemHeight, null);
                }
            }
        }

        private void PlayListPage_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer =SmallHelpers.FindScrollViewer(TrackListView);
            // throw new NotImplementedException();
        }
        ScrollViewer scrollViewer;
        private void desctruct()
        {
            AudioPlayer.onClickonTrack -= AudioPlayer_onClickonTrack;

            this.Loaded -= PlayListPage_Loaded;
            this.Unloaded -= PlayListPage_Unloaded;
           // Upload.UploadTrack.addedTrack -= addedTrack;
         //   if (scrollViewer != null) scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
           

        }

    }
}
