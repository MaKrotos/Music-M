using Microsoft.Extensions.DependencyInjection;
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
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using VK_UI3.Controllers;
using VK_UI3.DB;
using VK_UI3.Services;
using VK_UI3.Views.LoginWindow;
using VK_UI3.VKs;
using Windows.Foundation;
using static VK_UI3.DB.AccountsDB;
using static VK_UI3.Views.SectionView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page, INotifyPropertyChanged
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
            createNavigation();
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


        public ObservableCollection<NavSettings> navSettings = new ObservableCollection<NavSettings>();

        private async void createNavigation()
        {
            var catalogs = await VK.vkService.GetAudioCatalogAsync();
            var updatesSection = await VK.vkService.GetAudioCatalogAsync("https://vk.com/audio?section=updates");

            if (updatesSection.Catalog?.Sections?.Count > 0)
            {
                var section = updatesSection.Catalog.Sections[0];
                section.Title = "Подписки";
                catalogs.Catalog.Sections.Insert(catalogs.Catalog.Sections.Count - 1, section);
            }

            var sectionsService = StaticService.Container.GetRequiredService<ICustomSectionsService>();

            catalogs.Catalog.Sections.AddRange(await sectionsService.GetSectionsAsync().ToArrayAsync());

            var icons = new List<Symbol>
                {
                   Symbol.MusicInfo,
                   Symbol.More,
                   Symbol.Pictures,
                   Symbol.Map,

                };

            var rand = new Random();

            foreach (var section in catalogs.Catalog.Sections)
            {
                Symbol icon;

                if (section.Title.ToLower() == "главная")
                {
                    icon = Symbol.Home;
                }
                else if (section.Title.ToLower() == "моя музыка")
                {
                    icon = Symbol.Audio;
                }
                else if (section.Title.ToLower() == "обзор")
                {
                    icon = Symbol.PreviewLink;
                }
                else if (section.Title.ToLower() == "подкасты")
                {
                    icon = Symbol.Microphone;
                }
                else if (section.Title.ToLower() == "подписки")
                {
                    icon = Symbol.Favorite;
                }
                else if (section.Title.ToLower() == "каталоги")
                {
                    icon = Symbol.Library;
                }
                else if (section.Title.ToLower() == "поиск")
                {
                    icon = Symbol.Find;
                }

                else if (section.Title.ToLower().StartsWith("книги"))
                {
                    continue;
                }
                else
                {
                    var number = rand.Next(0, icons.Count);
                    icon = icons[number];
                    icons.RemoveAt(number);
                }



                if (section.Title.ToLower() == "моя музыка") section.Title = "Музыка";

                var navSet = new NavSettings() { Icon = icon, MyMusicItem = section.Title, section = section };
                navSettings.Add(navSet);

                //  var viewModel = ActivatorUtilities.CreateInstance<SectionViewModel>(StaticService.Container);
                //  viewModel.SectionId = section.Id;

                //  var navigationItem = new NavigationBarItem() { Tag = section.Id, PageDataContext = viewModel, Icon = icon, Content = section.Title, PageType = typeof(SectionView) };
                //  navigationBar.Items.Add(navigationItem);




            }


            int index = NavWiv.MenuItems.IndexOf(NavWiv.MenuItems.OfType<NavigationViewItemHeader>().First());
            foreach (var setting in navSettings)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    var navViewItem = new NavMenuController
                    {
                        navSettings = setting,
                        Content = setting.MyMusicItem,
                        Icon = new SymbolIcon(setting.Icon)

                    };
                    NavWiv.MenuItems.Insert(index, navViewItem);
                    index++;
                });
            }


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


        private async void ListViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {



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
                mainWindow = (MainWindow)navigationInfo.SourcePageType;
            }
            updateAccounts();
        }

        public static void updateAccounts()
        {
            Accounts.Clear();
            var accounts = AccountsDB.GetAllAccountsSorted();
            int i = 0;
            foreach (var item in accounts)
            {

                Accounts.Add(item);
                if (item.Active)
                {
                    //AccountsList.SelectedIndex = i;
                }
                i++;
            }
            Accounts.Add(new Accounts { });
        }

        MainWindow mainWindow = null;

        private int previousSelectedAccount = -1;

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedAccount = (Accounts)AccountsList.SelectedItem;
            // selectedAccount.itemSelected();
            if (selectedAccount == null) return;
            if (selectedAccount.Token == null)
            {

                // Создайте объект NavigationInfo и установите исходную страницу
                //  var navigationInfo = new NavigationInfo { SourcePageType = this };



                //  PopupFrame.Navigate(typeof(Login), navigationInfo, new DrillInNavigationTransitionInfo());
                //  CustomPopup.IsOpen = true;
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

                AccountsDB.ActivateAccount(selectedAccount.id);
                activeAccount = selectedAccount;
                if (ContentFrame.Content != null)
                {
                    var a = ContentFrame.Content.GetType();
                    ContentFrame.Navigate(a, this, new DrillInNavigationTransitionInfo());
                }
                else
                {
                    OpenMyPage(SectionType.MyListAudio);
                }

            }



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

                        //  ContentFrame.Navigate(typeof(MainMenu), null, new DrillInNavigationTransitionInfo());
                        ContentFrame.BackStack.Clear();
                        NavWiv.IsBackEnabled = false;
                        break;

                    case "параметры":

                        ContentFrame.BackStack.Clear();
                        NavWiv.IsBackEnabled = false;
                        break;

                    default:
                        var Item = sender.SelectedItem as NavMenuController;
                        OpenSection(Item.navSettings.section.Id);


                        ContentFrame.BackStack.Clear();
                        NavWiv.IsBackEnabled = false;
                        // RemoveNavItems();
                        break;
                        // и так далее...
                      
                }
              
            }
            else
            {




            }
           
          //  NavWiv.IsBackEnabled = ContentFrame.CanGoBack;
        }

        private void OpenMyPage(SectionType sectionType)
        {
            var sectionView = new WaitView();
            sectionView.sectionType = sectionType;
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }

        public static void OpenSection(string sectionID, SectionType sectionType = SectionType.None)
        {
            var sectionView = new WaitView();
            sectionView.SectionID = sectionID;
            sectionView.sectionType = sectionType;
            
            frame.Navigate(typeof(WaitView), sectionView, new DrillInNavigationTransitionInfo());
        }
       
    }

   

    public class NavigationInfo
    {
        public Object SourcePageType { get; set; }
        // Добавьте здесь другие свойства, если это необходимо

    }
}
