using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VK_UI3.VKs;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.LoginFrames
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

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            BackBTN.IsEnabled = this.Frame.CanGoBack;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {

            loginInvike();
        }

        private void loginInvike()
        {

            
                VK.LoginAsync(LoginTextBox.Text, PasswordBox.Password, this);
         
        }

        public async Task<string> InputTextDialogAsyncCapthca(Uri imageUrl)
        {
            string result = "";

            TextBox inputTextBox = new TextBox();
            Image captchaImage = new Image();
            BitmapImage bitmapImage = new BitmapImage(imageUrl);

            captchaImage.Source = bitmapImage;

            if (inputTextBox != null)
            {
                ContentDialog dialog = new ContentDialog();
                inputTextBox.AcceptsReturn = false;
                inputTextBox.Height = 32;
                // Добавьте обработчик событий KeyDown
                inputTextBox.KeyDown += (sender, e) =>
                {
                    if (e.Key == VirtualKey.Enter)
                    {
                        // Выполните действие, которое вы хотите выполнить при нажатии Enter
                        result = inputTextBox.Text;
                        dialog.Hide(); // Закройте диалог
                    }
                };

                StackPanel panel = new StackPanel();
                panel.Children.Add(captchaImage);
                panel.Children.Add(inputTextBox);

                

                if (dialog != null)
                {
                    dialog.XamlRoot = this.XamlRoot; // Associate with the correct XamlRoot
                    dialog.Content = panel;
                    dialog.Title = "Введите капчу";
                    dialog.IsSecondaryButtonEnabled = true;
                    dialog.PrimaryButtonText = "Подтвердить";
                    dialog.SecondaryButtonText = "Отмена";
                  
                        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                            result = inputTextBox.Text;
                
                }
            }

            return result;
        }



        public async Task<string> InputTextDialogAsync()
        {
            string result = "";

            TextBox inputTextBox = new TextBox();

            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            // Добавьте обработчик событий KeyDown
            ContentDialog dialog = new ContentDialog();
            inputTextBox.KeyDown += (sender, e) =>
            {
                if (e.Key == VirtualKey.Enter)
                {
                    // Выполните действие, которое вы хотите выполнить при нажатии Enter
                    result = inputTextBox.Text;
                    dialog.Hide();
                }
            };



            dialog.XamlRoot = XamlRoot; // Associate with the correct XamlRoot
            dialog.Content = inputTextBox;
            dialog.Title = "Введите код двухфакторной аутиентификации";
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

                ContentDialog dialog = new ContentDialog();

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
            this.Frame.GoBack();
        }

        private void LoginTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                PasswordBox.Focus(FocusState.Programmatic);
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                loginInvike();
            }
        }
    }
}
