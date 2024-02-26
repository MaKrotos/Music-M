using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VK_UI3.Helpers;
using VK_UI3.VKs;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class RecommsPlaylistBlock : UserControl
    {
        public RecommsPlaylistBlock()
        {
            this.InitializeComponent();

            DataContextChanged += RecommsPlaylistBlock_DataContextChanged;
            this.Loading += RecommsPlaylistBlock_Loading;
            this.Loaded += RecommsPlaylistBlock_Loaded;
        }

        private void RecommsPlaylistBlock_Loaded(object sender, RoutedEventArgs e)
        {
       
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

        private void RecommsPlaylistBlock_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            
        }
    }
}
