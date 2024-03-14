using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Helpers;
using VK_UI3.Views.ModalsPages;
using VK_UI3.VKs;
using VkNet.Model.Attachments;
using VkNet.Utils;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public  enum OpenedPlayList
    {
        all,
        UserPlayList,
        UserAlbums
    }
    public sealed partial class UserPlayList : Microsoft.UI.Xaml.Controls.Page
    {
      



        public UserPlayList()
        {
            this.InitializeComponent();
            this.Loaded += UserPlayList_Loaded;
            this.Loading += UserPlayList_Loading;
            
        }
        public ObservableRangeCollection<AudioPlaylist> audioPlaylists = new ObservableRangeCollection<AudioPlaylist>();
        public VkCollection<AudioPlaylist> VKaudioPlaylists;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var userPlayList = e.Parameter as UserPlayList;
            if (userPlayList == null)
                return;

            offset = userPlayList.offset;
            VKaudioPlaylists = userPlayList.VKaudioPlaylists;
            UserId = userPlayList.UserId;
            LoadedAll = userPlayList.LoadedAll;
            openedPlayList = userPlayList.openedPlayList;
            if (openedPlayList != OpenedPlayList.UserPlayList)
            {
                CreateButton.Visibility = Visibility.Collapsed;
            }
            
        }
        public uint offset;
        public long UserId;
        public OpenedPlayList openedPlayList;

        private bool CheckIfAllContentIsVisible(ScrollViewer scrollViewer)
        {
            if (scrollViewer.ViewportHeight >= scrollViewer.ExtentHeight)
            {
                return true;
            }
            return false;
        }

        private void UserPlayList_Loading(FrameworkElement sender, object args)
        {

            if (LoadedAll)
            {

                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        ScrollViewer scrollViewer { get; set; }
        private void UserPlayList_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer = SmallHelpers.FindScrollViewer(gridV);

            addToList(VKaudioPlaylists);

            if (CheckIfAllContentIsVisible(scrollViewer))
            {
                loadMoreAsync();
            }
            this.scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
           
            

            var isAtBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 50;
            if (isAtBottom)
            {
                loadMoreAsync();
            }
        }

        void addToList(VkCollection<AudioPlaylist> VKaudioPlaylists)
        {
            audioPlaylists.AddRange(VKaudioPlaylists.Where(item =>
            {
                bool isUserPlaylist = item.OwnerId == AccountsDB.activeAccount.id && item.Original == null;
                return openedPlayList switch
                {
                    OpenedPlayList.all => true,
                    OpenedPlayList.UserPlayList => isUserPlaylist,
                    OpenedPlayList.UserAlbums => !isUserPlaylist,
                    _ => false
                };
            }));
        }


        public bool LoadedAll; 
        private async Task loadMoreAsync()
        {
            if (LoadedAll) return;
            LoadingIndicator.IsActive = true;

            var a = await VK.api.Audio.GetPlaylistsAsync(UserId, 100, offset);

            addToList(a);
            LoadingIndicator.IsActive = false;
            if (a.Count != 100)
            {
                LoadedAll = true;
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
    

            if (CheckIfAllContentIsVisible(scrollViewer))
            {
                loadMoreAsync();
            }
        }

       
        private void CreateButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Grid button = sender as Grid;
            if (button != null)
            {
                Popup popup;
                CreatePlayList dialogContent = new CreatePlayList();
                // dialogContent.CloseRequested += (s, e) => popup.IsOpen = false;

                popup = new Popup
                {
                    Child = dialogContent,
                    XamlRoot = this.XamlRoot,
                    IsOpen = true,
                };
                Point clickPosition = e.GetCurrentPoint(button).Position;
                Point buttonPosition = button.TransformToVisual(null).TransformPoint(new Point());
                double horizontalOffset = clickPosition.X + buttonPosition.X;
                double verticalOffset = clickPosition.Y + buttonPosition.Y;

                // Проверка, чтобы всплывающее окно не выходило за пределы окна
                double windowWidth = App.m_window.Bounds.Width;
                double windowHeight = App.m_window.Bounds.Height;

                var dialogContentA = dialogContent.GetFirstChild() as Grid;
                

                if (horizontalOffset + dialogContentA.ActualWidth > windowWidth)
                {
                    horizontalOffset = windowWidth - dialogContentA.ActualWidth;
                }
                if (verticalOffset + dialogContentA.ActualWidth > windowHeight)
                {
                    verticalOffset = windowHeight - dialogContentA.ActualHeight;
                }

                popup.HorizontalOffset = horizontalOffset;
                popup.VerticalOffset = verticalOffset;
            }
        }
    }
}
