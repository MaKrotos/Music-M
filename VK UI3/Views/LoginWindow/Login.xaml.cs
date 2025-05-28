using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using StatSlyLib.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Views.ModalsPages;
using VK_UI3.VKs;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.System;
using static VK_UI3.DB.AccountsDB;
using Image = Microsoft.UI.Xaml.Controls.Image;



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
        Helpers.Animations.AnimationsChangeImage animationsChangeImage { get; set; }
        
        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            var accounts = AccountsDB.GetAllAccounts();
            if (accounts.Count < 0 || !Frame.CanGoBack) BackBTN.Visibility = Visibility.Collapsed;
            // BackBTN.IsEnabled = this.Frame.CanGoBack;
            LoginTextBox.Focus(FocusState.Pointer);
            MainWindow.mainWindow.MainWindow_hideRefresh();

         
        }







        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var UserUniqID = DB.SettingsTable.GetSetting("UserUniqID");

                EventParams eventParams = new EventParams("userID", UserUniqID.settingValue);

                var packageVersion = Package.Current.Id.Version;
                var version = $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
                var listParams = new List<EventParams>
                {
                        eventParams,
                        new EventParams("Accounts Count", AccountsDB.GetAllAccounts().Count),
                        new EventParams("versionAPP", version),
                };


                Event @event = new Event("Enter Login VK", DateTime.Now, eventParams: listParams);
                _ = new VKMStatSly().SendEvent(@event);
            }
            catch { }

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

                    ContentDialog _currentDialog = new CustomDialog();

                    _currentDialog.Transitions = new TransitionCollection
                        {
                            new PopupThemeTransition()
                        };



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
                        _currentDialog.Title = "Введите капчу";
                        _currentDialog.IsSecondaryButtonEnabled = true;
                        _currentDialog.PrimaryButtonText = "Подтвердить";
                        _currentDialog.SecondaryButtonText = "Отмена";

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
            // Добавьте обработчик событий KeyDown
            ContentDialog dialog = new CustomDialog();

            dialog.Transitions = new TransitionCollection
            {
                new PopupThemeTransition()
            };




            inputTextBox.KeyDown += (sender, e) =>
            {
                if (e.Key == VirtualKey.Enter)
                {
                    // Выполните действие, которое вы хотите выполнить при нажатии Enter
                    result = inputTextBox.Text;
                    if (dialog != null)
                        dialog.Hide();
                }
            };



            dialog.XamlRoot = XamlRoot; // Associate with the correct XamlRoot
            dialog.Content = inputTextBox;
            dialog.Title = descr;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Подтвердить";
            dialog.SecondaryButtonText = "Отмена";

           

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                result = inputTextBox.Text;

            return result;
        }







        public async Task<string> DialogMessageShow(string Message)
        {
            string result = "";

            ContentDialog dialog = new CustomDialog();

            dialog.Transitions = new TransitionCollection
            {
                new PopupThemeTransition()
            };


            this.DispatcherQueue.TryEnqueue(async () =>
            {
                if (dialog != null)
                {
                    dialog.XamlRoot = this.XamlRoot; // Associate with the correct XamlRoot

                    dialog.Title = Message;

                    dialog.PrimaryButtonText = "Ок";

                    dialog.ShowAsync();
                }
            });
            return result;
        }




        private async void BackButton(object sender, RoutedEventArgs e)
        {
            // _ = await InputTextDialogAsync("hello!", this.XamlRoot);
            activeAccount = GetActiveAccount();
            if (activeAccount == null)
            {
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
                Task task = new Task(() =>
                {
                    loginInvike();

                });
                task.RunSynchronously();
                Frame.Navigate(typeof(waitPage), this, new DrillInNavigationTransitionInfo());
            }
        }


    }
}
