using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Resources;
using VK_UI3.Helpers;
using VkNet.Model.Attachments;


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

    public class PlayListTemplateSelector : DataTemplateSelector
    {
        public VibeControl VibeControlTemplate { get; set; }
        public PlaylistControl playlistControl { get; set; }

        protected override DataTemplate? SelectTemplateCore(object? item, DependencyObject container)
        {
            if (item is AudioPlaylist)
            {
                string key;
                if ((item as AudioPlaylist).meta != null && (item as AudioPlaylist).meta.View != null)
                {
                 key = (item as AudioPlaylist).meta.View;
                }
                else
                {
                    key = "default";

                }

                PlayListTemplate playListTemplate = new PlayListTemplate();
      
                playListTemplate.Resources.TryGetValue(key, out object resource);
                if (resource != null) 
                {
                    return resource as DataTemplate;
                }
                else
                    playListTemplate.Resources.TryGetValue("default", out resource);
                return resource as DataTemplate;
            }

            return null;
        }
    }



    /*
     * 
     * 
   public class BlockTemplateSelector : DataTemplateSelector
 {
     protected override DataTemplate? SelectTemplateCore(object? item, DependencyObject container)
     {
         if (item is Block block)
         {
             BlockControl blockControl = new BlockControl();
             string key = string.IsNullOrEmpty(block.Layout?.Name) ? block.DataType : $"{block.DataType}_{block.Layout.Name}";

             if (blockControl.Resources.TryGetValue(key, out object resource) || blockControl.Resources.TryGetValue(block.DataType, out resource) || blockControl.Resources.TryGetValue("default", out resource))
             {
                 return resource as DataTemplate;
             }
         }

         return null;
     }
 }
     * 
     * 
     */
}
