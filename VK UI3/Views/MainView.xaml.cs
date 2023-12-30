using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using VK_UI3.DB;
using VK_UI3.Views.LoginWindow;
using static VK_UI3.DB.AccountsDB;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page
    {

        public MainView()
        {
            this.InitializeComponent();

           // FramePlayer.RenderTransform = trans;




            ContentFrame.Navigate(typeof(MainMenu), null, new DrillInNavigationTransitionInfo());
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
  
            // Проверьте, является ли переданный параметр типом NavigationInfo
            if (e.Parameter is NavigationInfo navigationInfo)
            {
                // Используйте navigationInfo здесь
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

        private void NavWiv_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var invokedItem = args.InvokedItem as string;
            switch (invokedItem)
            {
                case "Nav Item A":
                    // Навигация к странице A

                    ContentFrame.Navigate(typeof(MainMenu), null, new DrillInNavigationTransitionInfo());



                    break;
                case "Nav Item B":
                    // Навигация к странице B
                    break;
                case "Nav Item C":
                    // Навигация к странице C
                    break;
                default:
                    break;
                    // и так далее...
            }
        }

        // Объект TranslateTransform, который будет использоваться для анимации
        TranslateTransform trans = new TranslateTransform();

        // Storyboard, который будет использоваться для управления анимацией
        Storyboard sb = new Storyboard();

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
    }


    public class NavigationInfo
    {
        public Object SourcePageType { get; set; }
        // Добавьте здесь другие свойства, если это необходимо

    }
}
