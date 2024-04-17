using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SetupLib;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    public sealed partial class UpdatePage : Page
    {
        private AppUpdater appUpdater;

        public UpdatePage()
        {
            this.InitializeComponent();

            this.Unloaded += UpdatePage_Unloaded;
        }

        private void UpdatePage_Unloaded(object sender, RoutedEventArgs e)
        {
            appUpdater.DownloadProgressChanged -= AppUpdater_DownloadProgressChanged;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            appUpdater = e.Parameter as AppUpdater;


            // Update the UI with the new version details
            vers.Text = appUpdater.version;

            // Assuming you have a TextBlock for the release name and update description
            releaseName.Text = appUpdater.Name;
            updateDescription.Text = appUpdater.Tit;
            updateSize.Text = $"{Math.Round(appUpdater.sizeFile / 1024.0 / 1024, 2)} MB";

        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Subscribe to the DownloadProgressChanged event
            appUpdater.DownloadProgressChanged += AppUpdater_DownloadProgressChanged;
            UpdateBTN.IsEnabled = false;
            backBTN.IsEnabled = true;
            // Start the download and open the file
            await appUpdater.DownloadAndOpenFile(true);
        }

        private async void goBack(object sender, RoutedEventArgs e)
        {
            // Subscribe to the DownloadProgressChanged event
            this.Frame.GoBack();
        }

        private void AppUpdater_DownloadProgressChanged(object sender, AppUpdater.DownloadProgressChangedEventArgs e)
        {
            // Update the ProgressBar and the TextBlock with the download progress
            downloadProgressBar.Value = e.Percentage;
            downloadProgressText.Text = $"{Math.Round(e.BytesDownloaded / 1024.0 / 1024, 2)} МБ";

        }
    }
}
