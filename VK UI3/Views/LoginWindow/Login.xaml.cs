using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.VKs;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.System;
using static VK_UI3.DB.AccountsDB;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.LoginWindow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();


            
            //BackBTN.IsEnabled = this.Frame.CanGoBack;
            this.Loaded += Login_Loaded;


         

        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            var accounts = AccountsDB.GetAllAccounts();
            if (accounts.Count < 0 || !Frame.CanGoBack) BackBTN.Visibility = Visibility.Collapsed;
            // BackBTN.IsEnabled = this.Frame.CanGoBack;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {

            Task task = new Task(() =>
            {
                loginInvike();

            });
             task.RunSynchronously();

            

            Frame.Navigate(typeof(waitPage), this, new DrillInNavigationTransitionInfo());

        }

        private void loginInvike()
        {

            
                new VK(this).LoginAsync(LoginTextBox.Text);
                
         
        }

        public Task<string> InputTextDialogAsyncCapthca(Uri imageUrl)
        {

            var tcs = new TaskCompletionSource<string>();

            this.DispatcherQueue.TryEnqueue(() =>
            {
                

                TextBox inputTextBox = new TextBox();
                Image captchaImage = new Image();
                BitmapImage bitmapImage = new BitmapImage(imageUrl);

                captchaImage.Source = bitmapImage;

                if (inputTextBox != null)
                {
                    ContentDialog _currentDialog  = new ContentDialog();
                    inputTextBox.AcceptsReturn = false;
                    inputTextBox.Height = 32;
                    inputTextBox.KeyDown += (sender, e) =>
                    {
                        if (e.Key == VirtualKey.Enter)
                        {
                            tcs.SetResult(inputTextBox.Text);
                            _currentDialog.Hide();
                        }
                    };

                    StackPanel panel = new StackPanel();
                    panel.Children.Add(captchaImage);
                    panel.Children.Add(inputTextBox);

                    if (_currentDialog != null)
                    {
                        _currentDialog.XamlRoot = this.XamlRoot;
                        _currentDialog.Content = panel;
                        _currentDialog.Title = "Р’РІРµРґРёС‚Рµ РєР°РїС‡Сѓ";
                        _currentDialog.IsSecondaryButtonEnabled = true;
                        _currentDialog.PrimaryButtonText = "РџРѕРґС‚РІРµСЂРґРёС‚СЊ";
                        _currentDialog.SecondaryButtonText = "РћС‚РјРµРЅР°";

                        _currentDialog.ShowAsync().Completed += (info, status) =>
                        {
                            if (status == AsyncStatus.Completed && info.GetResults() == ContentDialogResult.Primary)
                            {
                                tcs.SetResult(inputTextBox.Text);
                            }
                            else
                            {
                                tcs.SetResult(string.Empty);
                            }
                        };
                    }
                }
            });

            return tcs.Task;
        }



        public async Task<string> InputTextDialogAsync(string descr)
        {
            string result = "";

            TextBox inputTextBox = new TextBox();

            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            // Р”РѕР±Р°РІСЊС‚Рµ РѕР±СЂР°Р±РѕС‚С‡РёРє СЃРѕР±С‹С‚РёР№ KeyDown
            ContentDialog dialog = new ContentDialog();
            inputTextBox.KeyDown += (sender, e) =>
            {
                if (e.Key == VirtualKey.Enter)
                {
                    // Р’С‹РїРѕР»РЅРёС‚Рµ РґРµР№СЃС‚РІРёРµ, РєРѕС‚РѕСЂРѕРµ РІС‹ С…РѕС‚РёС‚Рµ РІС‹РїРѕР»РЅРёС‚СЊ РїСЂРё РЅР°Р¶Р°С‚РёРё Enter
                    result = inputTextBox.Text;
                    dialog.Hide();
                }
            };



            dialog.XamlRoot = XamlRoot; // Associate with the correct XamlRoot
            dialog.Content = inputTextBox;
            dialog.Title = descr;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "РџРѕРґС‚РІРµСЂРґРёС‚СЊ";
            dialog.SecondaryButtonText = "РћС‚РјРµРЅР°";


            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                result = inputTextBox.Text;

            return result;
        }







        public async Task<string> DialogMessageShow(string Message)
        {
            string result = "";

                ContentDialog dialog = new ContentDialog();

            this.DispatcherQueue.TryEnqueue(async () =>
            {
                if (dialog != null)
                {
                    dialog.XamlRoot = this.XamlRoot; // Associate with the correct XamlRoot

                    dialog.Title = Message;

                    dialog.PrimaryButtonText = "РћРє";

                    dialog.ShowAsync();
                }
            });
            return result;
        }




        private async void BackButton(object sender, RoutedEventArgs e)
        {
            // _ = await InputTextDialogAsync("hello!", this.XamlRoot);
            activeAccount = GetActiveAccount();
            if (activeAccount == null) {
                var actacc = AccountsDB.GetAllAccounts();
                actacc[0].Active = true;
                actacc[0].Update();
            }
            this.Frame.Navigate(typeof(MainView));
        }

        private void LoginTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
               
                loginInvike();
            }
        }

       
    }
}

