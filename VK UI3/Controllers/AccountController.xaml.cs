using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using VK_UI3.DB;
using static VK_UI3.DB.AccountsDB;
using VK_UI3.Views;

namespace VK_UI3.Controllers
{
    public sealed partial class AccountController : UserControl
    {
       public static readonly DependencyProperty AccountsProperty = DependencyProperty.Register(
      "accounts", typeof(Accounts), typeof(AccountController), new PropertyMetadata(default(Accounts), onAccountProrertyChanged));
        public static readonly DependencyProperty ListAccs = DependencyProperty.Register(
     "ListAccs", typeof(ListView), typeof(AccountController), new PropertyMetadata(default(ListView), onAccountProrertyChanged));

        private static void onAccountProrertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public Accounts accounts
        {
            get { return (Accounts)GetValue(AccountsProperty); }
            set { SetValue(AccountsProperty, value); }
        }

        public Accounts listAccs
        {
            get { return (Accounts)GetValue(ListAccs); }
            set { SetValue(ListAccs, value); }
        }

        public AccountController()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) =>
            {
                Bindings.Update();
                if (accounts != null)
                    if (accounts.Token == null)
                    {
                        iconPic.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        iconPic.Visibility = Visibility.Collapsed;
                    }

            };

            Loaded += AccountController_Loaded;
        }

        private void AccountController_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void PersonPicture_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // Здесь добавьте код для удаления элемента
            DatabaseHandler.getConnect().Delete<Accounts>(this.accounts.id);
            MainView.updateAccounts();
        }
    }
}
