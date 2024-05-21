using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Collections.ObjectModel;
using static System.Net.Mime.MediaTypeNames;
using VK_UI3.VKs;


namespace VK_UI3.Controls.Blocks
{
    public sealed partial class ListPlaylists : UserControl
    {
        public ListPlaylists()
        {
            this.InitializeComponent();


            this.Loading += ListPlaylists_Loading;
            this.Loaded += ListPlaylists_Loaded;
            this.Unloaded += ListPlaylists_Unloaded;

        }

        private void ListPlaylists_Loaded(object sender, RoutedEventArgs e)
        {
            gridV.loadMore += loadMore;
            if (gridV.CheckIfAllContentIsVisible())
                load();
        }

        private void ListPlaylists_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= ListPlaylists_Unloaded;
            gridV.loadMore -= loadMore;
        }

        private void loadMore(object sender, EventArgs e)
        {
            load();
        }

        private async void load()
        {
            if (localBlock.NextFrom == null) return;
            var a = await VK.vkService.GetSectionAsync(localBlock.Id, localBlock.NextFrom);
            localBlock.NextFrom = a.Section.NextFrom;
            this.DispatcherQueue.TryEnqueue(async() => {
                foreach (var item in a.Playlists)
                {
                    playlists.Add(item);
                }
                if (gridV.CheckIfAllContentIsVisible())
                    load();
            });
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

        ObservableCollection<Playlist> playlists = new();
        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            gridV.ScrollRight();
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            gridV.ScrollLeft();
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
