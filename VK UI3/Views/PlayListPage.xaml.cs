using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VK_UI3.DB;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace VK_UI3.Views
{
    public record PlaylistData(long PlaylistId, long OwnerId, string AccessKey);
    public sealed partial class PlayListPage : Page
    {
        public PlayListPage()
        {
            this.InitializeComponent();
            this.Loaded += PlayListPage_Loaded;
          
        }
        public IVKGetAudio vkGetAudio = null;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            vkGetAudio = e.Parameter as IVKGetAudio;
            if (vkGetAudio == null)
            {
                vkGetAudio = new UserAudio(AccountsDB.activeAccount.id, this.DispatcherQueue);
            }



            SmallHelpers.AddImagesToGrid(GridThumbs, vkGetAudio.getPhotosList(), DispatcherQueue);
       



            vkGetAudio.onPhotoUpdated += VkGetAudio_onPhotoUpdated;
            vkGetAudio.onListUpdate += VkGetAudio_onListUpdate; ; ;

            base.OnNavigatedTo(e);
        }

        private void VkGetAudio_onListUpdate(object sender, EventArgs e)
        {
            blockLoad = false;
            if (vkGetAudio.itsAll)
                LoadingIndicator.Visibility = Visibility.Collapsed;
        }

    
        private void VkGetAudio_onPhotoUpdated(object sender, EventArgs e)
        {
            SmallHelpers.AddImagesToGrid(GridThumbs, vkGetAudio.getPhotosList(), DispatcherQueue);
        }

      
        private ScrollViewer FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer)
                return d as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var sw = FindScrollViewer(VisualTreeHelper.GetChild(d, i));
                if (sw != null) return sw;
            }

            return null;
        }
        ScrollViewer scrollViewer;
        private void PlayListPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Находим ScrollViewer внутри ListView

            scrollViewer = FindScrollViewer(TrackListView);
            if (scrollViewer != null)
            {
                // Подписываемся на событие изменения прокрутки
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged; ;
            }
        }

        bool blockLoad = false;
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            
            var isAtBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight;
            if (isAtBottom)
            {
               
                if (vkGetAudio.itsAll)
                {
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
                if (blockLoad) return;
                blockLoad = true;
                LoadingIndicator.Visibility = Visibility.Visible;
                vkGetAudio.GetTracks();
                LoadingIndicator.IsActive = true;
            }
        }

        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DownloadPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
