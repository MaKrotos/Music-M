using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using VK_UI3.VKs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class RemomendedUsersPlaylist : UserControl
    {
        public RemomendedUsersPlaylist()
        {
            this.InitializeComponent();


            this.Loading += RecommsPlaylistBlock_Loading;
            this.Unloaded += RecommsPlaylistBlock_Unloaded;
            myControl.loadMore = load;
        }

        private void RecommsPlaylistBlock_Unloaded(object sender, RoutedEventArgs e)
        {
            myControl.loadMore = null;
            this.Loading -= RecommsPlaylistBlock_Loading;
            this.Unloaded -= RecommsPlaylistBlock_Unloaded;
        }

        private void RecommsPlaylistBlock_Loading(FrameworkElement sender, object args)
        {
            try
            {
                if (DataContext is not Block block)
                    return;

                localBlock = block;

                var pl = (DataContext as Block).RecommendedPlaylists;


                foreach (var item in pl)
                {
                    playlists.Add(item);
                }
            }
            catch (Exception ex)
            {


            }
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
        Block localBlock;
        private async void load()
        {
            await semaphore.WaitAsync();
            try
            {
                if (localBlock.NextFrom == null) return;
                var a = await VK.vkService.GetSectionAsync(localBlock.Id, localBlock.NextFrom);
                localBlock.NextFrom = a.Section.NextFrom;
                this.DispatcherQueue.TryEnqueue(async () => {
                    foreach (var item in a.RecommendedPlaylists)
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


        ObservableCollection<RecommendedPlaylist> playlists = new();


    }
}
