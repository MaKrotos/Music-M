using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SetupLib;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    public sealed partial class UpdatePage : Page
    {
        private AppUpdater appUpdater;
        private UpdatePageViewModel viewModel;

        public UpdatePage(AppUpdater appUpdater) 
        {
            this.InitializeComponent();

            this.Unloaded += UpdatePage_Unloaded;
            this.Loaded += UpdatePage_Loaded;

            this.appUpdater = appUpdater;
            viewModel = new UpdatePageViewModel();
            this.DataContext = viewModel;
        }

        private void UpdatePage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateViewModelFromUpdater();
        }

        private void UpdatePage_Unloaded(object sender, RoutedEventArgs e)
        {
         //   appUpdater.DownloadProgressChanged -= AppUpdater_DownloadProgressChanged;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            appUpdater = e.Parameter as AppUpdater;
            UpdateViewModelFromUpdater();
        }

        private void UpdateViewModelFromUpdater()
        {
            if (appUpdater == null || viewModel == null) return;
            viewModel.Version = appUpdater.version;
            viewModel.ReleaseName = appUpdater.Name;
            viewModel.UpdateDescription = appUpdater.Tit;
            viewModel.UpdateSize = $"{Math.Round(appUpdater.sizeFile / 1024.0 / 1024, 2)} MB";
            viewModel.DownloadProgress = 0;
            viewModel.DownloadProgressText = "0 МБ";
        }

        

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Subscribe to the DownloadProgressChanged event
            appUpdater.DownloadProgressChanged += AppUpdater_DownloadProgressChanged;
            UpdateBTN.IsEnabled = false;


            // Start the download and open the file
            await appUpdater.DownloadAndOpenFile(true, false, PathInstallZIP: AppContext.BaseDirectory);
        }


        private void AppUpdater_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Update the ProgressBar and the TextBlock with the download progress
            if (viewModel != null)
            {
                viewModel.DownloadProgress = e.Percentage;
                viewModel.DownloadProgressText = $"{Math.Round(e.BytesDownloaded / 1024.0 / 1024, 2)} МБ";
            }
        }
    }

    public class UpdatePageViewModel : INotifyPropertyChanged
    {
        private string version;
        public string Version { get => version; set { version = value; OnPropertyChanged(); } }

        private string releaseName;
        public string ReleaseName { get => releaseName; set { releaseName = value; OnPropertyChanged(); } }

        private string updateDescription;
        public string UpdateDescription { get => updateDescription; set { updateDescription = value; OnPropertyChanged(); } }

        private string updateSize;
        public string UpdateSize { get => updateSize; set { updateSize = value; OnPropertyChanged(); } }

        private double downloadProgress;
        public double DownloadProgress { get => downloadProgress; set { downloadProgress = value; OnPropertyChanged(); } }

        private string downloadProgressText;
        public string DownloadProgressText { get => downloadProgressText; set { downloadProgressText = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
