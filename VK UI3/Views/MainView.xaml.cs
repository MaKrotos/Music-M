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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.DB;
using VK_UI3.Services;
using VK_UI3.Views.LoginWindow;
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

        public static Frame frame;
        public MainView()
        {
            this.InitializeComponent();


            // FramePlayer.RenderTransform = trans;
            frame = ContentFrame;

            //OpenMyPage(SectionType.MyListAudio);
            ContentFrame.Navigated += ContentFrame_Navigated;
            NavWiv.BackRequested += NavWiv_BackRequested;
            this.Loaded += MainView_Loaded;
            Accounts.CollectionChanged += Accounts_CollectionChanged;
            onUpdateAccounts += MainView_onUpdateAccounts;

            this.KeyDown += MainView_KeyDown;

        }

        private void MainView_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.F5)
            {
               MainWindow.onRefreshClickedvoid();
            }
    
        }

        private void Accounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
          //  throw new NotImplementedException();
        }

        private static DispatcherQueue dispatcherQueue = null;
        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            _ = CreateNavigation();
            dispatcherQueue = this.DispatcherQueue;

           MainWindow.mainWindow.MainWindow_showRefresh();
        }

        private void MainView_onUpdateAccounts(object sender, EventArgs e)
        {
            updateAccounts();
        }

        public static void invokeUpdateAccounts()
        {
            onUpdateAccounts?.Invoke(null, EventArgs.Empty);
        }
     

        public static event EventHandler onUpdateAccounts;

        private void NavWiv_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
                if (ContentFrame.CanGoBack)
                {
                    ContentFrame.GoBack();
                }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavWiv.IsBackEnabled = ContentFrame.CanGoBack;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                //   this.ImgUri = uri;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }


    
        public List<NavMenuController> navMenuControllers = new();
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


                    foreach (var setting in navSettings)
                    {
                        token.ThrowIfCancellationRequested();
                        this.DispatcherQueue.TryEnqueue(async() =>
                        {
                            var navViewItem = new NavMenuController
                            {
                                navSettings = setting,
                                Content = setting.MyMusicItem,
                                Icon = new SymbolIcon(setting.Icon)
                            };
                            NavWiv.MenuItems.Insert(index, navViewItem);
                            navMenuControllers.Add(navViewItem);
                            index++;
                        });
                    }


                }
                catch (OperationCanceledException)
                {
                    // Задача была отменена
                }


            }, token);
        }

        private void ClearMenuItems()
        {
            foreach (var item in navMenuControllers)
            {
                NavWiv.MenuItems.Remove(item);
            }
            navMenuControllers.Clear();
        }

        private List<Symbol> GetIcons()
        {
            return new List<Symbol>
    {
        Symbol.MusicInfo,
        Symbol.Audio,
        Symbol.Play,
        Symbol.Pause,
        Symbol.Stop,
        Symbol.Forward,
        Symbol.Back,
        Symbol.Previous,
        Symbol.Next,
        Symbol.Volume,
        Symbol.Mute,
        Symbol.More,
        Symbol.Pictures,
        Symbol.Map,
        Symbol.CalendarDay,
        Symbol.Bookmarks,
        };
            }

        private NavSettings CreateNavSettings(Section section, List<Symbol> icons)
        {
            Symbol icon;

            switch (section.Title.ToLower())
            {
                case "главная":
                    icon = Symbol.Home;
                    break;
                case "моя музыка":
                    icon = Symbol.Audio;
                    section.Title = "Музыка";
                    break;
                case "обзор":
                    icon = Symbol.PreviewLink;
                    break;
                case "подкасты":
                    icon = Symbol.Microphone;
                    break;
                case "подписки":
                    icon = Symbol.Favorite;
                    break;
                case "каталоги":
                    icon = Symbol.Library;
                    break;
                case "поиск":
                    icon = Symbol.Find;
                    break;
                case "книги и шоу":
                    icon = Symbol.Bookmarks;
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


        public static ObservableCollection<Accounts> Accounts { get; set; } = new ObservableCollection<Accounts>();
        

        ObservableCollection<Accounts> AccList
        {
            get { return Accounts; }
            set
            {
                Accounts = value;
            }
        }

        int getSelectedNumber
        {
            get
            {
                int a = -1;
                foreach (var item in Accounts)
                {
                    a++;
                    if (item.Active) return a;

                }
                return -1;
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
            updateAccounts();
        }

        public void updateAccounts()
        {
            Accounts.Clear();
            var accounts = AccountsDB.GetAllAccountsSorted();
            int i = 0;
            foreach (var item in accounts)
            {

                Accounts.Add(item);
                if (item.Active)
                {
                    AccountsList.SelectedIndex = i;
                }
                i++;
            }
            Accounts.Add(new Accounts { });
        }


        private int previousSelectedAccount = -1;


        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {

                var selectedAccount = (Accounts)AccountsList.SelectedItem;

                if (selectedAccount == null) return;
                if (selectedAccount.Token == null)
                {
                    if (previousSelectedAccount != -1)
                    {
                        AccountsList.SelectedItem = previousSelectedAccount;
                    }
                    else
                    {
                        AccountsList.SelectedIndex = -1;
                    }
                    AccountsDB.activeAccount = new AccountsDB.Accounts();
                    this.Frame.Navigate(typeof(Login), this, new DrillInNavigationTransitionInfo());
                    previousSelectedAccount = AccountsList.SelectedIndex;
                }
                else
                {

                    if (Accounts[AccountsList.SelectedIndex].id == AccountsDB.activeAccount.id) 
                        return;

                    AccountsDB.ActivateAccount(selectedAccount.id);
                    activeAccount = selectedAccount;
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
             
                
            });
        }





        private void ListViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            NavWiv.IsPaneOpen = false;

        }

        // Объект TranslateTransform, который будет использоваться для анимации
        TranslateTransform trans = new TranslateTransform();

        // Storyboard, который будет использоваться для управления анимацией
        Storyboard sb = new Storyboard();

        public event PropertyChangedEventHandler PropertyChanged;

        public void LowerFrame()
        {
            // Остановить текущую анимацию, если она выполняется
            if (sb.GetCurrentState() == ClockState.Active)
            {
                sb.Stop();
            }
            sb = new Storyboard();

            // Создайте новый объект DoubleAnimation
            DoubleAnimation da = new DoubleAnimation();

            // Установите начальное и конечное значения
            da.From = trans.Y; // начальное положение
                               //  da.To = FramePlayer.ActualHeight - 50; // конечное положение

            // Установите продолжительность анимации
            da.Duration = new Duration(TimeSpan.FromMilliseconds(250)); // продолжительность в секундах



            // Добавьте анимацию в Storyboard
            sb.Children.Add(da);

            // Установите целевой объект и свойство для анимации
            Storyboard.SetTarget(da, trans);
            Storyboard.SetTargetProperty(da, "Y");

            // Запустите анимацию
            sb.Begin();
        }


        public void RaiseFrame()
        {
            // Остановить текущую анимацию, если она выполняется
            if (sb.GetCurrentState() == ClockState.Active)
            {
                sb.Stop();

            }
            sb = new Storyboard();

            // Создайте новый объект DoubleAnimation
            DoubleAnimation da = new DoubleAnimation();

            // Установите начальное и конечное значения
            da.From = trans.Y; // начальное положение
            da.To = 0; // конечное положение (вернуться обратно)

            // Установите продолжительность анимации
            da.Duration = new Duration(TimeSpan.FromMilliseconds(250)); // продолжительность в секундах

            // Добавьте анимацию в Storyboard
            sb.Children.Add(da);

            // Установите целевой объект и свойство для анимации
            Storyboard.SetTarget(da, trans);
            Storyboard.SetTargetProperty(da, "Y");
           
            // Запустите анимацию
            sb.Begin();
        }





        private void FramePlayer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // RaiseFrame();

        }

        private void FramePlayer_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            //  LowerFrame();



        }

        private void NavWiv_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var invokedItem = sender.SelectedItem as NavigationViewItem;

            if (invokedItem == null)
            {
                sender.SelectedItem = sender.MenuItems[0];
                return;
            }

            if (invokedItem != null && invokedItem.Content != null)
            {
            
                switch (invokedItem.Content.ToString().ToLower())
                {
                    case "моя музыка":

                        OpenMyPage(SectionType.MyListAudio);
                    
                        break;

                    case "мои плейлисты":
                        OpenPlayListLists(openedPlayList: OpenedPlayList.UserPlayList);
                        break;

                    case "альбомы":
                        OpenPlayListLists(openedPlayList: OpenedPlayList.UserAlbums);
                        break;

                    case "параметры":

                        frame.Navigate(typeof(Settings.SettingsPage), null, new DrillInNavigationTransitionInfo());
                        break;

                    default:
                        var Item = sender.SelectedItem as NavMenuController;
                        OpenSection(Item.navSettings.section.Id);


                       
                        break;
                 
                      
                }

                ContentFrame.BackStack.Clear();
                NavWiv.IsBackEnabled = false;

            }
            else
            {




            }
           
          //  NavWiv.IsBackEnabled = ContentFrame.CanGoBack;
        }

        public static void OpenMyPage(SectionType sectionType)
        {
            var sectionView = new WaitView();
            sectionView.sectionType = sectionType;
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }


        public static void OpenPlayListLists(long? id = null, OpenedPlayList openedPlayList= OpenedPlayList.all)
        {
            var sectionView = new WaitView();
            if (id == null)
                id = activeAccount.id;
            sectionView.sectionType = SectionType.UserPlayListList;
            sectionView.SectionID = id.ToString();
            sectionView.openedPlayList = openedPlayList;
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }


        public static void OpenPlayList(AudioPlaylist playlist)
        {
            var sectionView = new WaitView();
            sectionView.sectionType = SectionType.PlayList;
            sectionView.Playlist = playlist;
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }

        public static void OpenPlayList(IVKGetAudio iVKGetAudio)
        {
            var sectionView = new WaitView();
            sectionView.sectionType = SectionType.PlayList;
            sectionView.iVKGetAudio = iVKGetAudio;
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }
       
        public static void OpenPlayList(long AlbumID, long AlbumOwnerID, string AlbumAccessKey)
        {
            var sectionView = new WaitView();
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
            var sectionView = new WaitView();
            sectionView.SectionID = sectionID;
            sectionView.sectionType = sectionType;
            
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }

        private void AccountsList_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            Task.Run(async () =>
            {
                var i = 0;
                foreach (var item in Accounts)
                {
                    if (item.Token == null) continue;
                    item.sortID = i++;
                    item.Update();

                    if (item.Active)
                        AccountsList.SelectedIndex = i;
                }
            });
        }

        private void AccountsList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var items = (sender as ListView).Items;
            if (e.Items.Contains(items[items.Count - 1]))
            {
                e.Cancel = true;
            }
        }
    }



    public class NavigationInfo
    {
        public Object SourcePageType { get; set; }
        // Добавьте здесь другие свойства, если это необходимо

    }
}
