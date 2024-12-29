using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MusicX.Core.Models;
using System;
using System.Linq;
using TagLib.Asf;
using VK_UI3.Controllers;
using VK_UI3.Helpers;
using VK_UI3.Views;
using VK_UI3.VKs.IVK;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public partial class ListTracks : UserControl, IBlockAdder
    {


        public ListTracks()
        {
            InitializeComponent();
            this.DataContextChanged += ListTracks_DataContextChanged;

            this.Loaded += ListTracks_Loaded;
            this.Unloaded += ListTracks_Unloaded;
        }

        private void ListTracks_Unloaded(object sender, RoutedEventArgs e)
        {
            this.DataContextChanged -= ListTracks_DataContextChanged;
            this.Loaded -= ListTracks_Loaded;
            this.Unloaded -= ListTracks_Unloaded;
            AudioPlayer.onClickonTrack -= AudioPlayer_onClickonTrack;
            if (connected)
            {
                sectionAudio.onListUpdate -= SectionAudio_onListUpdate;
                connected = false;
            }
            myControl.loadMore = null;

        }

    

        private void ListTracks_Loaded(object sender, RoutedEventArgs e)
        {
            AudioPlayer.onClickonTrack += AudioPlayer_onClickonTrack;
            try
            {
                sectionAudio?.NotifyOnListUpdate();
            }
            catch { }

            myControl.loadMore = sectionAudio.GetTracks;
            if (myControl.CheckIfAllContentIsVisible()) sectionAudio.GetTracks();
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
            SectionView sectionPage = (FindParentByName<Page>(this, "SectionPage") as SectionView);

            var audio = sender as ExtendedAudio;
            var ins = sectionAudio.listAudio.IndexOf(audio);

            if (ins < 0)
            {
                audio = sectionAudio.listAudio.FirstOrDefault(a => a.audio.Id == audio.audio.Id && a.audio.OwnerId == audio.audio.OwnerId);
                ins = sectionAudio.listAudio.IndexOf(audio);
            }

            if (ins >= 0 && ins < myControl.gridView.Items.Count)
            {
                var container = myControl.gridView.ContainerFromIndex(ins) as GridViewItem;
                if (container != null)
                {
                    var transform = container.TransformToVisual(myControl);
                    var position = transform.TransformPoint(new Point(0, 0));
                    double itemHeight = position.Y;
                    double itemWidth = position.X;

                    // Scroll vertically and horizontally
                    sectionPage.ScrollToElement(DataContext as Block, itemHeight);
                    myControl.scrollVi.ChangeView(myControl.scrollVi.HorizontalOffset + itemWidth -20, null, null);
                }
            }
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
                if (!connected)
                {
                    sectionAudio.onListUpdate += SectionAudio_onListUpdate;
                    connected = true;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void SectionAudio_onListUpdate(object sender, EventArgs e)
        {
            if (myControl.CheckIfAllContentIsVisible())
            {
                sectionAudio.GetTracks();
            }
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

        private SectionAudio sectionAudio;
    }
}

