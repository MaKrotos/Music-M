using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TagLib;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Share
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatList : Microsoft.UI.Xaml.Controls.Page
    {

        ObservableCollection<HistoryAttachment> attachments = new ObservableCollection<HistoryAttachment>();
        public ChatList()
        {
            this.InitializeComponent();
            this.Loaded += FriendsList_Loaded;
        }
        ScrollViewer scrollViewer;
        private void FriendsList_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer = FindScrollViewer(scrollView);
            if (scrollViewer != null)
            {
                // Подписываемся на событие изменения прокрутки
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged; ;
            }
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
            loadMoreFriends();
        }
        bool itsAll = false;
        bool loadingNow = false;

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var isAtBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 50;
            if (isAtBottom || CheckIfAllContentIsVisible(sender as ScrollViewer))
            {

                if (itsAll)
                {
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
             
                LoadingIndicator.Visibility = Visibility.Visible;
                loadMoreFriends();
                LoadingIndicator.IsActive = true;
            }
        }

        private bool CheckIfAllContentIsVisible(ScrollViewer scrollViewer)
        {
            if (scrollViewer.ViewportHeight >= scrollViewer.ExtentHeight)
            {
                return true;
            }
            return false;
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
        int counted = 0;
        private long peerID;
        string nextFrom = null;
        private async Task loadMoreFriends()
        {

         
            if (itsAll)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    LoadingIndicator.IsActive = false;
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                });
                return;
            }
            if (loadingNow) return;
             loadingNow = true;

            int off = 200;

            var parameters = new VkParameters
            {
                { "count", off },
                { "fields", "can_see_audio,photo_50,online, online, photo_100, photo_200_orig, status, nickname" },
                { "offset", counted },
                { "order", "name" }
            };
            counted += off;

            MessagesGetHistoryAttachmentsParams messagesGetHistoryAttachmentsParams = new MessagesGetHistoryAttachmentsParams()
            {
                Count = 200,
                MediaType = MediaType.Audio,
                PeerId = this.peerID,
                StartFrom = nextFrom
              

            };

            var a = (VK.api.Messages.GetHistoryAttachments(messagesGetHistoryAttachmentsParams, out nextFrom));
            

            if (a.Count < off) itsAll = true;

            this.DispatcherQueue.TryEnqueue(() =>
            {
                foreach (var item in a)
                {
                    attachments.Add(item);
                }
            });


            if (itsAll)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    LoadingIndicator.IsActive = false;
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            loadingNow = false;
                if (CheckIfAllContentIsVisible(scrollViewer)) loadMoreFriends();
        }
    }
}
