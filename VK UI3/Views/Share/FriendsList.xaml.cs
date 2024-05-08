using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Share
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class FriendsListParametrs
    {
        internal bool itsAll;
        internal VkCollection<User> friends;
    }
    public sealed partial class FriendsList : Page
    {

        ObservableCollection<User> friends = new ObservableCollection<User>();
        public FriendsList()
        {
            this.InitializeComponent();
            this.Loaded += FriendsList_Loaded;
        }
        ScrollViewer scrollViewer = null;
        private void FriendsList_Loaded(object sender, RoutedEventArgs e)
        {
            


            if (scrollViewer == null) scrollViewer = FindScrollViewer(scrollView);
            if (scrollViewer != null)
            {
                // Подписываемся на событие изменения прокрутки
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged; ;
            }
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
            if (friendsListParametrs != null)
            {
                friendsListParametrs.itsAll = itsAll;

                this.DispatcherQueue.TryEnqueue(() =>
                {
                    foreach (var item in friendsListParametrs.friends)
                    {
                        friends.Add(item);
                    }
                    if (scrollViewer == null) scrollViewer = FindScrollViewer(scrollView);
                    if (CheckIfAllContentIsVisible(scrollViewer)) loadMoreFriends();
                });
            }
            else
            {
                loadMoreFriends();
            }
           
        }
        bool itsAll = false;
        bool loadingNow = false;
        FriendsListParametrs friendsListParametrs = null;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var userPlayList = e.Parameter as FriendsListParametrs;
            if (userPlayList == null)
                return;
            friendsListParametrs = userPlayList;
        }
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

            int off = 25;

            var parameters = new VkParameters
            {
                { "count", off },
                { "fields", "can_see_audio,photo_50,online, online, photo_100, photo_200_orig, status, nickname" },
                { "offset", friends.Count },
                { "order", "name" }
            };

            var a = (await VK.api.CallAsync("friends.get", parameters)).ToVkCollectionOf<User>(
                x => parameters["fields"] != null
                      ? x
                      : new User
                      {
                          Id = x
                      }
                );

            if (a.Count < off) itsAll = true;

            this.DispatcherQueue.TryEnqueue(() =>
            {
                foreach (var item in a)
                {
                        friends.Add(item);
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
