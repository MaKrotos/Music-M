using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Collections.ObjectModel;
using static System.Net.Mime.MediaTypeNames;
using VK_UI3.VKs;
using System.Threading;
using VK_UI3.VKs.IVK;


namespace VK_UI3.Controls.Blocks
{
    public sealed partial class ListPlaylists : UserControl
    {

        ObservableCollection<Playlist> playlists = new();
        public ListPlaylists()
        {
            this.InitializeComponent();


            this.Loading += ListPlaylists_Loading;
            this.Loaded += ListPlaylists_Loaded;
            this.Unloaded += ListPlaylists_Unloaded;

        }

        private void ListPlaylists_Loaded(object sender, RoutedEventArgs e)
        {
            if (gridV.CheckIfAllContentIsVisible())
                load();
            gridV.loadMore += loadMore;
            gridV.LeftChange += LeftChange;
            gridV.RightChange += RightChange;
        }

        private void ListPlaylists_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= ListPlaylists_Unloaded;
            gridV.loadMore -= loadMore;

            gridV.loadMore -= loadMore;
            gridV.LeftChange -= LeftChange;
            gridV.RightChange -= RightChange;
        }


        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private async void load()
        {
            await semaphore.WaitAsync();
            try
            {
                if (localBlock.NextFrom == null) return;
                var a = await VK.vkService.GetSectionAsync(localBlock.Id, localBlock.NextFrom);
                localBlock.NextFrom = a.Section.NextFrom;
                this.DispatcherQueue.TryEnqueue(async () => {
                    foreach (var item in a.Playlists)
                    {
                        playlists.Add(item);
                    }
                    if (gridV.CheckIfAllContentIsVisible())
                        load();
                });
            }
            finally
            {
                semaphore.Release();
            }
        }


        Block localBlock;
        private void ListPlaylists_Loading(FrameworkElement sender, object args)
        {
            try
            {
                if (DataContext is not Block block)
                    return;
                playlists.Clear();
                localBlock = block;

                if (block.Meta != null && block.Meta.anchor == "vibes")

                {
                    gridV.ItemTemplate = this.Resources["compact"] as DataTemplate;
                }
                else
                {
                    gridV.ItemTemplate = this.Resources["default"] as DataTemplate;
                }

                var pl = (DataContext as Block).Playlists;


                foreach (var item in pl)
                {
                    playlists.Add(item);
                }
              
            }
            catch (Exception ex)
            {



            }
        }



        private void LeftChange(object sender, EventArgs e)
        {
            LeftCh();
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
                if (localBlock.NextFrom != null)
                {
                    FadeInAnimationRightBTN.Pause();
                    FadeOutAnimationRightBTN.Begin();
                }
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
        private void loadMore(object sender, EventArgs e)
        {
            load();
        }


        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            gridV.ScrollRight();
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            gridV.ScrollLeft();
        }

    }
}
