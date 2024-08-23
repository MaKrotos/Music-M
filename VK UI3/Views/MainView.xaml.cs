using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using MusicX.Core.Services;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.DB;
using VK_UI3.Helpers.Animations;
using VK_UI3.Services;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;
using static VK_UI3.DB.AccountsDB;
using static VK_UI3.Views.SectionView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Microsoft.UI.Xaml.Controls.Page, INotifyPropertyChanged
    {
        public static MainView mainView;
        public static Frame frame;
        public MainView()
        {
            this.InitializeComponent();


            // FramePlayer.RenderTransform = trans;
            frame = ContentFrame;
            mainView = this;

            //OpenMyPage(SectionType.MyListAudio);

            this.Loaded += MainView_Loaded;
            this.Loading += MainView_Loading;

        }

        private void MainView_Loading(FrameworkElement sender, object args)
        {
            NavWiv.IsPaneOpen = false;
        }

        public void updateAllWithReacreate()
        {


            TempPlayLists.TempPlayLists.updateNextRequest = true;
            if (ContentFrame.Content != null)
            {
                _ = CreateNavigation();
            }
            else
            {
                OpenMyPage(SectionType.MyListAudio);
            }
        }

        private void MainView_Unloaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigated -= ContentFrame_Navigated;
            NavWiv.BackRequested -= NavWiv_BackRequested;
            AccountsDB.ChanhgeActiveAccount -= ChangeAccount;
            this.KeyDown -= MainView_KeyDown;

            this.Unloaded -= MainView_Unloaded;
        }

        private void MainView_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.F5)
            {
                MainWindow.onRefreshClickedvoid();
            }
        }



        private static DispatcherQueue dispatcherQueue = null;
        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {

            NavWiv.IsPaneOpen = false;
            //OpenMyPage(SectionType.MyListAudio);
            ContentFrame.Navigated += ContentFrame_Navigated;

            NavWiv.BackRequested += NavWiv_BackRequested;

            MainWindow.mainWindow.onBackClicked += back;

            AccountsDB.ChanhgeActiveAccount += ChangeAccount;


            this.KeyDown += MainView_KeyDown;

            this.Unloaded += MainView_Unloaded;

            _ = CreateNavigation();
            dispatcherQueue = this.DispatcherQueue;

            MainWindow.mainWindow.MainWindow_showRefresh();
        }

        private async void back(object sender, EventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (ContentFrame.CanGoBack)
                    ContentFrame.GoBack();
            });
        }

        private void ChangeAccount(object sender, EventArgs e)
        {
            updateAllWithReacreate();
        }



        private void NavWiv_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (ContentFrame.CanGoBack && !navToAnotherPage)
            {
                MainWindow.mainWindow.backBTNShow();

            }
            else
            {
                MainWindow.mainWindow.backBTNHide();
            }
            navToAnotherPage = false;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                //   this.ImgUri = uri;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }



        public List<AnimatedNavMenuController> navMenuControllers = new();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        //   private CancellationTokenSource cts = null;
        private async Task CreateNavigation()
        {
            // Отменить предыдущую задачу, если она была запущена
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;


            _ = Task.Run(async () =>
            {
                try
                {


                    ObservableCollection<NavSettings> navSettings = new ObservableCollection<NavSettings>();

                    this.DispatcherQueue.TryEnqueue(async () =>
                    {
                        NavWiv.SelectedItem = 0;
                        navigateInvoke();
                        ClearMenuItems();
                    });
                    var catalogs = await VK.vkService.GetAudioCatalogAsync();


                    token.ThrowIfCancellationRequested();
                    var updatesSection = await VK.vkService.GetAudioCatalogAsync("https://vk.com/audio?section=updates");

                    token.ThrowIfCancellationRequested();
                    if (updatesSection.Catalog?.Sections?.Count > 0)
                    {
                        var section = updatesSection.Catalog.Sections[0];
                        section.Title = "Подписки";
                        catalogs.Catalog.Sections.Insert(catalogs.Catalog.Sections.Count - 1, section);
                    }



                    var sectionsService = StaticService.Container.GetRequiredService<ICustomSectionsService>();
                    catalogs.Catalog.Sections.AddRange(await sectionsService.GetSectionsAsync().ToArrayAsync());

                    var icons = GetIcons();

                    token.ThrowIfCancellationRequested();

                    foreach (var section in catalogs.Catalog.Sections)
                    {
                        var navSet = CreateNavSettings(section, icons);
                        navSettings.Add(navSet);
                    }
                    int index = 0;
                    this.DispatcherQueue.TryEnqueue(async () =>
                    {
                        index = NavWiv.MenuItems.IndexOf(NavWiv.MenuItems.OfType<NavigationViewItemHeader>().First());
                    });

                    token.ThrowIfCancellationRequested();

                    this.DispatcherQueue.TryEnqueue(async () =>
                    {
                        foreach (var setting in navSettings)
                        {
                            token.ThrowIfCancellationRequested();

                            var navViewItem = new AnimatedNavMenuController
                            {
                                navSettings = setting,
                                Content = setting.MyMusicItem,
                                Icon = new FontIcon { Glyph = setting.Icon }
                            };
                            NavWiv.MenuItems.Insert(index, navViewItem);
                            navMenuControllers.Add(navViewItem);
                            index++;

                        }
                    });


                }
                catch (OperationCanceledException)
                {
                    // Задача была отменена
                }
                catch (Exception e)
                {

                }


            }, token);
        }

        private void ClearMenuItems()
        {
            foreach (var item in navMenuControllers)
            {

                item.deleted += OnItemDeleted;


                item.delete();
            }
            navMenuControllers.Clear();
        }


        private void OnItemDeleted(object sender, EventArgs e)
        {
            // Отписываемся от события
            ((AnimatedNavMenuController)sender).deleted -= OnItemDeleted;
            NavWiv.MenuItems.Remove(sender);
        }

        private List<string> GetIcons()
        {
            try
            {
                return new List<string>
        {
            "\uE142", // MusicInfo
            "\uE189", // Audio
            "\uE102", // Play
            "\uE103", // Pause
            "\uE101", // Stop
            "\uE100", // Forward
            "\uE106", // Back
            "\uE108", // Previous
            "\uE107", // Next
            "\uE767", // Volume
            "\uE74F", // Mute
            "\uE10C", // More
            "\uE158", // Pictures
            "\uE707", // Map
            "\uE787", // CalendarDay
            "\uE141", // Bookmarks
        };
            }
            catch (Exception e)
            {
                return null;
            }
        }


        private NavSettings CreateNavSettings(Section section, List<string> icons)
        {
            string icon;

            switch (section.Title.ToLower())
            {
                case "главная":
                    icon = "\uE10F"; // Home
                    break;
                case "моя музыка":
                    icon = "\uE189"; // Audio
                    section.Title = "Музыка";
                    break;
                case "обзор":
                    icon = "\uECA5"; // PreviewLink
                    break;
                case "подкасты":
                    icon = "\uE1D6"; // Microphone
                    break;
                case "подписки":
                    icon = "\uE734"; // Favorite
                    break;
                case "каталоги":
                    icon = "\uE1D3"; // Library
                    break;
                case "поиск":
                    icon = "\uE11A"; // Find
                    break;
                case "книги и шоу":
                    icon = "\uE82D"; // Bookmarks
                    break;
                default:
                    icon = icons[0];
                    icons.RemoveAt(0);
                    break;
            }

            return new NavSettings() { Icon = icon, MyMusicItem = section.Title, section = section };
        }



        public async void RemoveNavItems()
        {
            var separator = NavWiv.MenuItems.OfType<NavigationViewItemSeparator>().First();
            var header = NavWiv.MenuItems.OfType<NavigationViewItemHeader>().First();

            int startIndex = NavWiv.MenuItems.IndexOf(separator);
            int endIndex = NavWiv.MenuItems.IndexOf(header);

            for (int i = endIndex; i > startIndex; i--)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    NavWiv.MenuItems.RemoveAt(i);
                });
            }
        }





        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //OpenMyPage(SectionType.MyListAudio);
            // Проверьте, является ли переданный параметр типом NavigationInfo
            if (e.Parameter is NavigationInfo navigationInfo)
            {
                // Используйте navigationInfo здесь
                // ((MainWindow) navigationInfo.SourcePageType).GoLogin();
                MainWindow.mainWindow = (MainWindow)navigationInfo.SourcePageType;
            }

        }





        private void ListViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
          //  NavWiv.IsPaneOpen = false;

        }

        // Объект TranslateTransform, который будет использоваться для анимации
        TranslateTransform trans = new TranslateTransform();

        // Storyboard, который будет использоваться для управления анимацией
        Storyboard sb = new Storyboard();

        public event PropertyChangedEventHandler PropertyChanged;




        bool navToAnotherPage = false;
  

        public static void OpenMyPage(SectionType sectionType)
        {
            var sectionView = new WaitParameters();
            sectionView.sectionType = sectionType;
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }


        public static void OpenPlayListLists(long? id = null, OpenedPlayList openedPlayList = OpenedPlayList.all)
        {
            var sectionView = new WaitParameters();
            if (id == null)
                id = activeAccount.id;
            sectionView.sectionType = SectionType.UserPlayListList;
            sectionView.SectionID = id.ToString();
            sectionView.openedPlayList = openedPlayList;
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }


        public static void OpenPlayList(AudioPlaylist playlist)
        {
            var waitParameters = new WaitParameters();
            waitParameters.sectionType = SectionType.PlayList;
            waitParameters.Playlist = playlist;
            frame.Navigate(typeof(WaitView), waitParameters, new DrillInNavigationTransitionInfo());
        }

        public static void OpenPlayList(IVKGetAudio iVKGetAudio)
        {
                var sectionView = new WaitParameters();
                sectionView.sectionType = SectionType.PlayList;
                sectionView.iVKGetAudio = iVKGetAudio;
                frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }

        public static void OpenPlayList(long AlbumID, long AlbumOwnerID, string AlbumAccessKey)
        {
            var sectionView = new WaitParameters();
            sectionView.sectionType = SectionType.PlayList;
            Playlist playlist = new Playlist();
            playlist.Id = AlbumID;
            playlist.OwnerId = AlbumOwnerID;
            playlist.AccessKey = AlbumAccessKey;
            PlayListVK playListVK = new PlayListVK(playlist, dispatcherQueue);
            sectionView.iVKGetAudio = playListVK;
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }

        public static void OpenSection(string sectionID, SectionType sectionType = SectionType.None)
        {
            var sectionView = new WaitParameters();
            sectionView.SectionID = sectionID;
            if (sectionView.SectionID != "search")
            {
                sectionView.sectionType = sectionType;
            }
            else
            {
                sectionView.sectionType = SectionType.Search;
            }
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }

        public static void OpenSearchSection(string searchString)
        {
            var sectionView = new WaitParameters();
            sectionView.searchText = searchString;
            sectionView.SectionID = "search";
        
            sectionView.sectionType = SectionType.Search;
 
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }


        public void SearchSetText(string Text)
        {
            Search.searchtext = Text;
        }

        public void showSearch() 
        {
            Search.Show();
        }

        public void hideSearch()
        {
            Search.Hide();
        }

        private void NavWiv_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            navigateInvoke();
        }

        private void navigateInvoke()
        {
            var invokedItem = NavWiv.SelectedItem as NavigationViewItem;

            if (invokedItem == null)
            {
                NavWiv.SelectedItem = NavWiv.MenuItems[0];
                invokedItem = NavWiv.SelectedItem as NavigationViewItem;
            }

            if (invokedItem != null && invokedItem.Content != null)
            {

                navToAnotherPage = true;
                switch (invokedItem.Content.ToString().ToLower())
                {
                    case "моя музыка":

                        OpenMyPage(SectionType.MyListAudio);

                        break;

                    case "мои плейлисты":
                        OpenPlayListLists(openedPlayList: OpenedPlayList.UserPlayList);
                        break;

                    case "плейлисты":
                        OpenPlayListLists(openedPlayList: OpenedPlayList.UserAlbums);
                        break;

                    case "параметры":
                        mainView.hideSearch();
                        frame.Navigate(typeof(Settings.SettingsPage), null, new DrillInNavigationTransitionInfo());
                        break;

                    case "музыка друзей":
                        OpenMyPage(SectionType.LoadFriends);
                        break;

                    case "вложения":
                        OpenMyPage(SectionType.ConversDialogs);
                        break;


                    default:
                        var Item = NavWiv.SelectedItem as NavMenuController;
                        OpenSection(Item.navSettings.section.Id);



                        break;


                }


                ContentFrame.BackStack.Clear();
                MainWindow.mainWindow.backBTNHide();

            }
            else
            {




            }
        }

      
    }



    public class NavigationInfo
    {
        public Object SourcePageType { get; set; }
        // Добавьте здесь другие свойства, если это необходимо

    }
}
