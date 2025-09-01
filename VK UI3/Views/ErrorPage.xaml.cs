using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace VK_UI3.Views
{
    public sealed partial class ErrorPage : Page
    {
        public event EventHandler RetryRequested;

        public ErrorPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string errorMessage)
            {
                SetError(errorMessage);
            }
        }

        // Метод для установки сообщения об ошибке
        public void SetError(string errorMessage)
        {
            ErrorDetailsText.Text = errorMessage;
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.onRefreshClickedvoid();
        }

        private async void CopyErrorButton_Click(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(ErrorDetailsText.Text);
            Clipboard.SetContent(dataPackage);

            CopyErrorButton.Content = "Скопировано!";
            await System.Threading.Tasks.Task.Delay(2000);
            CopyErrorButton.Content = "Копировать ошибку";
        }
    }
}