using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Collections.ObjectModel;
using VK_UI3.Helpers;


namespace VK_UI3.Controls.Blocks
{
    public sealed partial class ListPlaylists : UserControl
    {
        public ListPlaylists()
        {
            this.InitializeComponent();

      
            this.Loading += ListPlaylists_Loading;

            this.Unloaded += ListPlaylists_Unloaded;
        }

        private void ListPlaylists_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loading -= ListPlaylists_Loading;

            this.Unloaded -= ListPlaylists_Unloaded;
        }

        private void ListPlaylists_Loading(FrameworkElement sender, object args)
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
