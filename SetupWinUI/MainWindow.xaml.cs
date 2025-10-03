using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SetupLib;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI.WindowManagement;

namespace SetupWinUI
{
    public sealed partial class MainWindow : Window
    {
        private AppUpdater appUpdater;
        private string installLog = "";
        private const int MaxWindowWidth = 800; // Максимальная ширина окна
        Microsoft.UI.Windowing.AppWindow m_AppWindow = null;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Activated += MainWindow_Activated;

            // Ограничение максимальной ширины окна
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(MaxWindowWidth, appWindow.Size.Height));
            appWindow.Changed += (s, e) =>
            {
                if (appWindow.Size.Width > MaxWindowWidth)
                {
                    appWindow.Resize(new SizeInt32(MaxWindowWidth, appWindow.Size.Height));
                    SetRegionsForCustomTitleBar();
                }
            };
            m_AppWindow = this.AppWindow;
            // Включаем системный TitleBar
            appWindow.Title = "Установщик VK M";
            InitializeWindowProperties();

        }

        private void InitializeWindowProperties()
        {
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
            SetRegionsForCustomTitleBar();
        }


        private void SetRegionsForCustomTitleBar()
        {
            // Specify the interactive regions of the title bar.
            if (AppTitleBar.XamlRoot == null)
                return;
            double scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;
            var nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(m_AppWindow.Id);


            // Получите все дочерние элементы StackPanel
            var children = AppTitleBar.Children;

            // Создайте список для хранения прямоугольников каждого элемента
            List<Windows.Graphics.RectInt32> rects = new List<Windows.Graphics.RectInt32>();

            foreach (var child in children)
            {
                var frameworkElement = child as FrameworkElement;
                // Пропустите TitIcon и AppTitle
                if (frameworkElement.Name == "TitIcon" || frameworkElement.Name == "AppTitle")
                    continue;

                // Получите границы каждого элемента
                var transform = child.TransformToVisual(null);
                var bounds = transform.TransformBounds(new Rect(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight));

                // Преобразуйте координаты DPI

                var rect = new Windows.Graphics.RectInt32(
                    _X: (int)Math.Round(bounds.X * scaleAdjustment),
                    _Y: (int)Math.Round(bounds.Y * scaleAdjustment),
                    _Width: (int)Math.Round(bounds.Width * scaleAdjustment),
                    _Height: (int)Math.Round(bounds.Height * scaleAdjustment)
                );

                // Добавьте прямоугольник в список
                rects.Add(rect);
            }

            // Установите области, которые будут прозрачными для кликов мыши
            nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rects.ToArray());


        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.Activated -= MainWindow_Activated;
            await StartAsync();
        }

        private async System.Threading.Tasks.Task StartAsync()
        {
            try
            {
                appUpdater = new AppUpdater("0");
                appUpdater.InstallStatusChanged += AppUpdater_InstallStatusChanged;
                var a = await appUpdater.CheckForUpdates();

                label10.Text = appUpdater.version;
                label9.Text = appUpdater.Name;
                whatsNews.Text = appUpdater.Tit;
                label7.Text = Math.Round((float)appUpdater.sizeFile / 1024 / 1024, 2).ToString() + " Мб";
                label10.Text = appUpdater.version;
                button1.IsEnabled = true;
                progressBar1.IsIndeterminate = false;
                if (appUpdater._currentReleaseInfo.Assets.ContainsKey(PackageType.MSIX))
                {
                    MSIXRadio.IsEnabled = true;
                    MSIXRadio.IsChecked = true;
                }
                if (appUpdater._currentReleaseInfo.Assets.ContainsKey(PackageType.ZIP))
                {
                    if (MSIXRadio.IsChecked != true)
                    {
                        MSIXRadio.IsChecked = true;
                    }
                    EXERadio.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Ошибка при проверке обновлений: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task ShowMessageDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Ошибка",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void AppUpdater_InstallStatusChanged(object sender, InstallStatusChangedEventArgs e)
        {
            installLog += e.Status + "\r\n";
            logTextBox.Text = installLog;
            logTextBox.SelectionStart = logTextBox.Text.Length;
            // logTextBox.ScrollToEnd(); // Not available in WinUI 3, so skip
        }

        private void AppUpdater_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.Percentage;
            label6.Text = Math.Round((float)e.BytesDownloaded / 1024 / 1024, 2).ToString() + " Мб";
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                button1.IsEnabled = false;
                MSIXRadio.IsEnabled = false;
                EXERadio.IsEnabled = false;

                bool a = appUpdater.IsVersionInstalled(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);

                if (!a)
                {
                    var dialog = new ContentDialog
                    {
                        Title = ".NET не установлен",
                        Content = $"Необходимо установить .NET версии минимум {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}",
                        PrimaryButtonText = "Установить",
                        CloseButtonText = "Отмена",
                        XamlRoot = this.Content.XamlRoot
                    };
                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        bool winget_installed = appUpdater.CheckIfWingetIsInstalled();
                        if (winget_installed)
                        {
                            appUpdater.InstallLatestDotNetAppRuntime();
                        }
                        else
                        {
                            await ShowMessageDialog("Отсуствуют некоторые компоненты для автоматической установки .NET После установки приложения, .NET необходимо будет установить вручную.");
                        }
                    }
                }

                appUpdater.DownloadProgressChanged += AppUpdater_DownloadProgressChanged;
                await appUpdater.DownloadAndOpenFile(forceInstall: checkBox1.IsChecked == true);
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Ошибка при установке: " + ex.Message);
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://t.me/VK_M_creator",
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }

        private void Radio_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).Name == MSIXRadio.Name)
            {
                appUpdater.SelectedPackageType = PackageType.MSIX;
            }
            else
            {
                appUpdater.SelectedPackageType = PackageType.ZIP;
            }
            label7.Text = Math.Round((float)appUpdater.sizeFile / 1024 / 1024, 2).ToString() + " Мб";
        }
    }
}
