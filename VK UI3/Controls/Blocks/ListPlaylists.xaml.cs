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
                AppCenterHelper.SendCrash(ex);


            }
        }

        ObservableCollection<Playlist> playlists = new();


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
