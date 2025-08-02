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
    public sealed partial class ListPlaylists : UserControl, IBlockAdder
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
                if (a.Playlists == null) return;
                this.DispatcherQueue.TryEnqueue(async () => {
                    foreach (var item in a.Playlists)
                    {
                        playlists.Add(item);
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
                playlists.Clear();
                localBlock = block;

                switch (localBlock.Layout.Name)
                {
                    case "list":
                        myControl.disableLoadMode = true;
                        break;
                    default:
                        myControl.loadMore = load;
                        myControl.ItemsPanelTemplate = (ItemsPanelTemplate)Microsoft.UI.Xaml.Application.Current.Resources["GlobalItemsPanelTemplate"];

                        break;
                }

                if (block.Meta != null && (block.Meta.anchor == "vibes" || block.Meta.anchor == "genres"))

                {
                    myControl.ItemTemplate = this.Resources["compact"] as DataTemplate;
                }
                else
                {
                    myControl.ItemTemplate = this.Resources["default"] as DataTemplate;
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

        public void AddBlock(Block block)
        {
            localBlock.NextFrom = block.NextFrom;
            this.DispatcherQueue.TryEnqueue(async () => {
                foreach (var item in block.Playlists)
                {
                    playlists.Add(item);
                }
            });
        }
    }
}
