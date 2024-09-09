using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Collections.ObjectModel;
using static System.Net.Mime.MediaTypeNames;
using VK_UI3.VKs;
using System.Threading;
using VK_UI3.VKs.IVK;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;


namespace VK_UI3.Controls.Blocks
{
    public sealed partial class ListVideo : UserControl, IBlockAdder
    {

        ObservableCollection<Video> video = new();

     


        public ListVideo()
        {
            this.InitializeComponent();


            this.Loading += ListPlaylists_Loading;
            this.Loaded += ListPlaylists_Loaded;
            this.Unloaded += ListPlaylists_Unloaded;

        }
     

        private void ListPlaylists_Loaded(object sender, RoutedEventArgs e)
        {

            if (localBlock.Layout.Name == "categories_list")
            {

                myControl.scrollVi.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                myControl.scrollVi.HorizontalScrollMode = ScrollMode.Disabled;
                myControl.scrollVi.IsScrollInertiaEnabled = false;
                myControl.scrollVi.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                myControl.scrollVi.VerticalScrollMode = ScrollMode.Disabled;
            }
            else
            {
             
            }

            if (myControl.CheckIfAllContentIsVisible())
                load();

        }

        private void ListPlaylists_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= ListPlaylists_Unloaded;
            myControl.loadMore = null;

        }


        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public bool itsAll
        {
            get
            {
                if (localBlock == null) return true;
                if (localBlock.NextFrom == null) return true; else return false;
            }
        }
        private async void load()
        {
            await semaphore.WaitAsync();
            try
            {
                if (localBlock.NextFrom == null) return;
                var a = await VK.vkService.GetSectionAsync(localBlock.Id, localBlock.NextFrom);
                localBlock.NextFrom = a.Section.NextFrom;
                this.DispatcherQueue.TryEnqueue(async () => {
                    if (a.Videos == null) return;
                    foreach (var item in a.Videos)
                    {
                        video.Add(item);
                    }
                    if (myControl.CheckIfAllContentIsVisible())
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
                video.Clear();
                localBlock = block;
                switch (localBlock.Layout.Name)
                {
                    case "list":
                        myControl.disableLoadMode = true;
     
                        break;
                    default:
                        myControl.loadMore = load;

                        myControl.ItemsPanelTemplate = (ItemsPanelTemplate)Microsoft.UI.Xaml.Application.Current.Resources["GlobalItemsPanelTemplate"];
                        myControl.disableLoadMode = false;
                        break;
                }


                // Применяем DataTemplate к свойству ItemTemplate UniversalControl



                var pl = (DataContext as Block).Videos;
                foreach (var item in pl)
                {
                    video.Add(item);
                }

                pl = (DataContext as Block).ArtistVideos;
                foreach (var item in pl)
                {
                    video.Add(item);
                }

               


            }
            catch (Exception ex)
            {



            }
        }

        public void AddBlock(Block block)
        {
            localBlock.NextFrom = block.NextFrom;
        
            this.DispatcherQueue.TryEnqueue(async () => {
                foreach (var item in block.Videos)
                {
                    video.Add(item);
                }
                foreach (var item in block.ArtistVideos)
                {
                    video.Add(item);
                }

            });
        }
    }
}
