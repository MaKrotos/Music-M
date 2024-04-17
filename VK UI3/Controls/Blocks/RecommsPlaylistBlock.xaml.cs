using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Collections.ObjectModel;
using VK_UI3.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class RecommsPlaylistBlock : UserControl
    {
        public RecommsPlaylistBlock()
        {
            this.InitializeComponent();


            this.Loading += RecommsPlaylistBlock_Loading;
            this.Unloaded += RecommsPlaylistBlock_Unloaded;
        }

        private void RecommsPlaylistBlock_Unloaded(object sender, RoutedEventArgs e)
        {

            this.Loading -= RecommsPlaylistBlock_Loading;
            this.Unloaded -= RecommsPlaylistBlock_Unloaded;
        }

        private void RecommsPlaylistBlock_Loading(FrameworkElement sender, object args)
        {
            try
            {
                if (DataContext is not Block block)
                    return;

                var pl = (DataContext as Block).Playlists;


                foreach (var item in pl)
                {
                    playlists.Add(item);
                }
            }
            catch (Exception ex)
            {
                AppCenterHelper.SendCrash(ex);


            }
        }

        ObservableCollection<Playlist> playlists = new();


    }
}
