using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VK_UI3.Controllers;
using VK_UI3.Helpers;
using VK_UI3.Views;
using VK_UI3.VKs.IVK;
using Windows.Foundation;



namespace VK_UI3.Controls.Blocks
{
    public partial class ListTracksFull : UserControl, IBlockAdder
    {


        public ListTracksFull()
        {
            InitializeComponent();

            this.DataContextChanged += ListTracks_DataContextChanged;
            this.Loaded += ListTracksFull_Loaded;
            this.Unloaded += ListTracksFull_Unloaded;

        }

        private void ListTracksFull_Loaded(object sender, RoutedEventArgs e)
        {
            AudioPlayer.onClickonTrack += AudioPlayer_onClickonTrack;
        }

        private T FindParentByName<T>(DependencyObject child, string name) where T : FrameworkElement
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            while (parentObject != null)
            {
                T parent = parentObject as T;
                if (parent != null && parent.Name == name)
                {
                    return parent;
                }
                parentObject = VisualTreeHelper.GetParent(parentObject);
            }

            return null;
        }



        private void AudioPlayer_onClickonTrack(object sender, EventArgs e)
        {

            SectionView sectionPage = (FindParentByName<Page>(this, "SectionPage") as SectionView) ;
           


            var audio = sender as ExtendedAudio;
            var ins = sectionAudio.listAudio.IndexOf(audio);

            if (ins < 0)
            {
                audio = sectionAudio.listAudio.FirstOrDefault(a => a.audio.Id == audio.audio.Id && a.audio.OwnerId == audio.audio.OwnerId);
                ins = sectionAudio.listAudio.IndexOf(audio);
            }

            if (ins >= 0 && ins < TrackListView.Items.Count)
            {
                var container = TrackListView.ContainerFromIndex(ins) as ListViewItem;
                if (container != null)
                {
                    var transform = container.TransformToVisual(TrackListView);
                    var position = transform.TransformPoint(new Point(0, 0));
                    double itemHeight = position.Y;

                    //scrollViewer.ChangeView(null, , null);
                    sectionPage.ScrollToElement(DataContext as Block, itemHeight);
                    //sectionPage.ScrollToElement(DataContext as Block, scrollViewer.VerticalOffset + itemHeight);
                }
            }
        }

        private void ListTracksFull_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ListTracksFull_Loaded;
            AudioPlayer.onClickonTrack -= AudioPlayer_onClickonTrack;
            this.DataContextChanged -= ListTracks_DataContextChanged;
            try
            {
                if (sectionAudio != null)
                    sectionAudio.onListUpdate -= SectionAudio_onListUpdate;
            }
            catch { }
            this.Unloaded -= ListTracksFull_Unloaded;
            connected = false;
        }
        bool connected = false;

        private void ListTracks_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (DataContext is not Block block)
                    return;

                sectionAudio = new SectionAudio(block, this.DispatcherQueue);
                sectionAudio.countTracks = block.Audios.Count;
                OnPropertyChanged(nameof(sectionAudio));

                if (!connected)
                {
                    connected = true;
                    sectionAudio.onListUpdate += SectionAudio_onListUpdate;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public SectionAudio sectionAudio;


        protected void OnPropertyChanged(string propertyName)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                //   this.ImgUri = uri;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            });

        }
        private void SectionAudio_onListUpdate(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(sectionAudio));
        }

        public void AddBlock(Block block)
        {
            foreach (var item in block.Audios)
            {
                sectionAudio.listAudio.Add(new ExtendedAudio(item, sectionAudio));
                
            }
            sectionAudio.countTracks = sectionAudio.listAudio.Count;
            sectionAudio.Next = block.NextFrom;
        }

        public static readonly DependencyProperty TracksProperty =
         DependencyProperty.Register("Tracks", typeof(List<Audio>), typeof(ListTracks), new PropertyMetadata(new List<Audio>()));

        public event PropertyChangedEventHandler PropertyChanged;

        public List<Audio> Tracks
        {
            get { return (List<Audio>)GetValue(TracksProperty); }
            set
            {
                SetValue(TracksProperty, value);
            }
        }



    }
}

