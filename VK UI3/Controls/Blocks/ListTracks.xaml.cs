using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using VK_UI3.Helpers;
using VK_UI3.VKs.IVK;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public partial class ListTracks : UserControl
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
            if (connected)
            {
                sectionAudio.onListUpdate -= SectionAudio_onListUpdate;
                connected = false;
            }
            gridV.loadMore -= loadMore;
            gridV.LeftChange -= LeftChange;
            gridV.RightChange -= RightChange;
        }

        private void loadMore(object sender, EventArgs e)
        {
            sectionAudio.GetTracks();
        }

        private void ListTracks_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                sectionAudio?.NotifyOnListUpdate();
            }
            catch { }
            gridV.loadMore += loadMore;

            gridV.LeftChange += LeftChange;
            gridV.RightChange += RightChange;
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
            if (gridV.CheckIfAllContentIsVisible())
            {
                sectionAudio.GetTracks();
            }
        }

        private SectionAudio sectionAudio;

      
        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            gridV.ScrollRight();
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            gridV.ScrollLeft();
        }



        private void RightChange(object sender, EventArgs e)
        {
            RightCh();
        }

        private void RightCh()
        {
            if (gridV.showRight)
            {
                RightGrid.Visibility = Visibility.Visible;

                FadeOutAnimationRightBTN.Pause();
                FadeInAnimationRightBTN.Begin();

            }
            else
            {
                if (sectionAudio.itsAll)
                {
                    FadeInAnimationRightBTN.Pause();
                    FadeOutAnimationRightBTN.Begin();
                }
            }
        }

        private void LeftChange(object sender, EventArgs e)
        {
            LeftCh();
        }

        private void LeftCh()
        {
            if (gridV.showLeft)
            {
                LeftGrid.Visibility = Visibility.Visible;

                FadeOutAnimationLeftBTN.Pause();
                FadeInAnimationLeftBTN.Begin();
            }
            else
            {
                FadeInAnimationLeftBTN.Pause();
                FadeOutAnimationLeftBTN.Begin();

            }
        }

        private void FadeOutAnimationRightBTN_Completed(object sender, object e)
        {
            if (!gridV.showRight || !enterpoint)
            {
                RightGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void FadeOutAnimationLeftBTN_Completed(object sender, object e)
        {
            if (!gridV.showLeft || !enterpoint)
            {
                LeftGrid.Visibility = Visibility.Collapsed;
            }
           
        }

        private void gridCh_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            enterpoint = true;
            var a = gridV.ShowLeftChecker;
            a = gridV.ShowRightChecker;
            LeftCh();
            RightCh();
        }
        bool enterpoint;
        private void gridCh_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            this.enterpoint = false;
            FadeInAnimationLeftBTN.Pause();
            FadeOutAnimationLeftBTN.Begin();
            FadeInAnimationRightBTN.Pause();
            FadeOutAnimationRightBTN.Begin();
        }
    }
}

