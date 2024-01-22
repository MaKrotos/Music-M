using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page, INotifyPropertyChanged
    {

        public MainView()
        {
            this.InitializeComponent();


            // FramePlayer.RenderTransform = trans;


            ContentFrame.Navigate(typeof(MainMenu), null, new DrillInNavigationTransitionInfo());

            createNavigation();
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
                section.Title = "РџРѕРґРїРёСЃРєРё";
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

               if (section.Title.ToLower() == "РіР»Р°РІРЅР°СЏ")
                {
                    icon = Symbol.Home;
                }
                else if (section.Title.ToLower() == "РјРѕСЏ РјСѓР·С‹РєР°")
                {
                    icon = Symbol.Audio;
                }
                else if (section.Title.ToLower() == "РѕР±Р·РѕСЂ")
                {
                    icon = Symbol.PreviewLink;
                }
                else if (section.Title.ToLower() == "РїРѕРґРєР°СЃС‚С‹")
                {
                    icon = Symbol.Microphone;
                }
                else if (section.Title.ToLower() == "РїРѕРґРїРёСЃРєРё")
                {
                    icon = Symbol.Favorite;
                }
                else if (section.Title.ToLower() == "РєР°С‚Р°Р»РѕРіРё")
                {
                    icon = Symbol.Library;
                }
                else if (section.Title.ToLower() == "РїРѕРёСЃРє")
                {
                    icon = Symbol.Find;
                }

                else if (section.Title.ToLower().StartsWith("РєРЅРёРіРё"))
                {
                    continue;
                }
                else
                {
                    var number = rand.Next(0, icons.Count);
                    icon = icons[number];
                    icons.RemoveAt(number);
                }



                if (section.Title.ToLower() == "РјРѕСЏ РјСѓР·С‹РєР°") section.Title = "РњСѓР·С‹РєР°";

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

        ObservableCollection<Accounts> AccList { 
            get { return Accounts; }
             set { Accounts = value; 
            }
        }

        int getSelectedNumber
        {
            get {
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
  
            // РџСЂРѕРІРµСЂСЊС‚Рµ, СЏРІР»СЏРµС‚СЃСЏ Р»Рё РїРµСЂРµРґР°РЅРЅС‹Р№ РїР°СЂР°РјРµС‚СЂ С‚РёРїРѕРј NavigationInfo
            if (e.Parameter is NavigationInfo navigationInfo)
            {
                // РСЃРїРѕР»СЊР·СѓР№С‚Рµ navigationInfo Р·РґРµСЃСЊ
                // ((MainWindow) navigationInfo.SourcePageType).GoLogin();
                mainWindow = (MainWindow) navigationInfo.SourcePageType;
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

        private int previousSelectedAccount =-1;

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedAccount = (Accounts)AccountsList.SelectedItem;
            // selectedAccount.itemSelected();
            if (selectedAccount == null) return;
                if (selectedAccount.Token == null)
            {

                // РЎРѕР·РґР°Р№С‚Рµ РѕР±СЉРµРєС‚ NavigationInfo Рё СѓСЃС‚Р°РЅРѕРІРёС‚Рµ РёСЃС…РѕРґРЅСѓСЋ СЃС‚СЂР°РЅРёС†Сѓ
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
            else {

                AccountsDB.ActivateAccount(selectedAccount.id);
                activeAccount = selectedAccount;
                var a = ContentFrame.Content.GetType(); 
                ContentFrame.Navigate(a, this, new DrillInNavigationTransitionInfo());
       
            }

      

        }
        
  



        private void ListViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            NavWiv.IsPaneOpen = false;

        }

        // РћР±СЉРµРєС‚ TranslateTransform, РєРѕС‚РѕСЂС‹Р№ Р±СѓРґРµС‚ РёСЃРїРѕР»СЊР·РѕРІР°С‚СЊСЃСЏ РґР»СЏ Р°РЅРёРјР°С†РёРё
        TranslateTransform trans = new TranslateTransform();

        // Storyboard, РєРѕС‚РѕСЂС‹Р№ Р±СѓРґРµС‚ РёСЃРїРѕР»СЊР·РѕРІР°С‚СЊСЃСЏ РґР»СЏ СѓРїСЂР°РІР»РµРЅРёСЏ Р°РЅРёРјР°С†РёРµР№
        Storyboard sb = new Storyboard();

        public event PropertyChangedEventHandler PropertyChanged;

        public void LowerFrame()
        {
            // РћСЃС‚Р°РЅРѕРІРёС‚СЊ С‚РµРєСѓС‰СѓСЋ Р°РЅРёРјР°С†РёСЋ, РµСЃР»Рё РѕРЅР° РІС‹РїРѕР»РЅСЏРµС‚СЃСЏ
            if (sb.GetCurrentState() == ClockState.Active)
            {
                sb.Stop();
            }
            sb = new Storyboard();

            // РЎРѕР·РґР°Р№С‚Рµ РЅРѕРІС‹Р№ РѕР±СЉРµРєС‚ DoubleAnimation
            DoubleAnimation da = new DoubleAnimation();

            // РЈСЃС‚Р°РЅРѕРІРёС‚Рµ РЅР°С‡Р°Р»СЊРЅРѕРµ Рё РєРѕРЅРµС‡РЅРѕРµ Р·РЅР°С‡РµРЅРёСЏ
            da.From = trans.Y; // РЅР°С‡Р°Р»СЊРЅРѕРµ РїРѕР»РѕР¶РµРЅРёРµ
          //  da.To = FramePlayer.ActualHeight - 50; // РєРѕРЅРµС‡РЅРѕРµ РїРѕР»РѕР¶РµРЅРёРµ

            // РЈСЃС‚Р°РЅРѕРІРёС‚Рµ РїСЂРѕРґРѕР»Р¶РёС‚РµР»СЊРЅРѕСЃС‚СЊ Р°РЅРёРјР°С†РёРё
            da.Duration = new Duration(TimeSpan.FromMilliseconds(250)); // РїСЂРѕРґРѕР»Р¶РёС‚РµР»СЊРЅРѕСЃС‚СЊ РІ СЃРµРєСѓРЅРґР°С…

          

            // Р”РѕР±Р°РІСЊС‚Рµ Р°РЅРёРјР°С†РёСЋ РІ Storyboard
            sb.Children.Add(da);

            // РЈСЃС‚Р°РЅРѕРІРёС‚Рµ С†РµР»РµРІРѕР№ РѕР±СЉРµРєС‚ Рё СЃРІРѕР№СЃС‚РІРѕ РґР»СЏ Р°РЅРёРјР°С†РёРё
            Storyboard.SetTarget(da, trans);
            Storyboard.SetTargetProperty(da, "Y");

            // Р—Р°РїСѓСЃС‚РёС‚Рµ Р°РЅРёРјР°С†РёСЋ
            sb.Begin();
        }


        public void RaiseFrame()
        {
            // РћСЃС‚Р°РЅРѕРІРёС‚СЊ С‚РµРєСѓС‰СѓСЋ Р°РЅРёРјР°С†РёСЋ, РµСЃР»Рё РѕРЅР° РІС‹РїРѕР»РЅСЏРµС‚СЃСЏ
            if (sb.GetCurrentState() == ClockState.Active)
            {
                sb.Stop();
                
            }
            sb = new Storyboard();

            // РЎРѕР·РґР°Р№С‚Рµ РЅРѕРІС‹Р№ РѕР±СЉРµРєС‚ DoubleAnimation
            DoubleAnimation da = new DoubleAnimation();

            // РЈСЃС‚Р°РЅРѕРІРёС‚Рµ РЅР°С‡Р°Р»СЊРЅРѕРµ Рё РєРѕРЅРµС‡РЅРѕРµ Р·РЅР°С‡РµРЅРёСЏ
            da.From = trans.Y; // РЅР°С‡Р°Р»СЊРЅРѕРµ РїРѕР»РѕР¶РµРЅРёРµ
            da.To = 0; // РєРѕРЅРµС‡РЅРѕРµ РїРѕР»РѕР¶РµРЅРёРµ (РІРµСЂРЅСѓС‚СЊСЃСЏ РѕР±СЂР°С‚РЅРѕ)

            // РЈСЃС‚Р°РЅРѕРІРёС‚Рµ РїСЂРѕРґРѕР»Р¶РёС‚РµР»СЊРЅРѕСЃС‚СЊ Р°РЅРёРјР°С†РёРё
            da.Duration = new Duration(TimeSpan.FromMilliseconds(250)); // РїСЂРѕРґРѕР»Р¶РёС‚РµР»СЊРЅРѕСЃС‚СЊ РІ СЃРµРєСѓРЅРґР°С…
           
            // Р”РѕР±Р°РІСЊС‚Рµ Р°РЅРёРјР°С†РёСЋ РІ Storyboard
            sb.Children.Add(da);

            // РЈСЃС‚Р°РЅРѕРІРёС‚Рµ С†РµР»РµРІРѕР№ РѕР±СЉРµРєС‚ Рё СЃРІРѕР№СЃС‚РІРѕ РґР»СЏ Р°РЅРёРјР°С†РёРё
            Storyboard.SetTarget(da, trans);
            Storyboard.SetTargetProperty(da, "Y");

            // Р—Р°РїСѓСЃС‚РёС‚Рµ Р°РЅРёРјР°С†РёСЋ
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
                    case "РјРѕСЏ РјСѓР·С‹РєР°":
                        ContentFrame.Navigate(typeof(MainMenu), null, new DrillInNavigationTransitionInfo());
                        break;

                    case "РїР°СЂР°РјРµС‚СЂС‹":

                    break;

                    default:
                        var Item = sender.SelectedItem as NavMenuController;

                        getsect(Item.navSettings.section.Id, Item.navSettings.section.NextFrom);

                        // RemoveNavItems();
                        break;
                        // Рё С‚Р°Рє РґР°Р»РµРµ...
                }
            }
            else
            {
            



            }
        }

        private async void getsect(string section, string from)
        {
            //var a= await VK.vkService.GetSectionAsync(section, from);
            //ContentFrame.Navigate(typeof(MainMenu), null, new DrillInNavigationTransitionInfo());
        }
    }


    public class NavigationInfo
    {
        public Object SourcePageType { get; set; }
        // Р”РѕР±Р°РІСЊС‚Рµ Р·РґРµСЃСЊ РґСЂСѓРіРёРµ СЃРІРѕР№СЃС‚РІР°, РµСЃР»Рё СЌС‚Рѕ РЅРµРѕР±С…РѕРґРёРјРѕ

    }
}
