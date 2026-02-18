#define SKIP_ERROR_PRONE_CODE
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SetupLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.DownloadTrack;
using VK_UI3.Helpers;
using VK_UI3.Views;
using VK_UI3.Views.LoginWindow;
using VK_UI3.Views.Notification;
using VK_UI3.Views.Upload;
using VK_UI3.VKs;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Media;
using Windows.UI.ViewManagement;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinRT.Interop;

namespace VK_UI3
{
    public sealed partial class MainWindow : Microsoft.UI.Xaml.Window
    {
        #region Fields and Properties
        internal static HWND hvn;
        AppWindow m_AppWindow = null;
        public static MainWindow mainWindow;
        public static DispatcherQueue dispatcherQueue { get; private set; }
        public static Frame contentFrame;
        public static EventHandler updateMica;
        public static EventHandler onRefreshClicked;
        public EventHandler onBackClicked;
        public static WeakEventManager onDownloadClicked = new WeakEventManager();
        public static DownloadFileWithProgress downloadFileWithProgress = null;

        private Microsoft.Win32.SafeHandles.SafeFileHandle iIcon;

        private delegate IntPtr WinProc(HWND hWnd, uint Msg, WPARAM wParam, IntPtr lParam);
        private WinProc newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;
        private bool addClosed = false;
        public bool justClose = false;
        #endregion

        #region Constants
        private const uint WM_GETMINMAXINFO = 0x0024;
        #endregion

        #region Initialization
        public MainWindow()
        {
            InitializeSystemIntegration();

            InitializeComponent();
            InitializeWindowProperties();
            InitializeEventHandlers();
            InitializeUIComponents();
            InitializeNavigation();


        }

        private void InitializeWindowProperties()
        {
            m_AppWindow = this.AppWindow;
            mainWindow = this;
            contentFrame = ContentFrame;
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
        }

        private void InitializeEventHandlers()
        {
            this.Closed += MainWindow_CloseRequested;
            AppTitleBar.Loaded += AppTitleBar_Loaded;
            AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
            ContentFrame.Navigated += ContentFrame_Navigated;

            PlayListDownload.OnEndAllDownload += OnEndAllDownload_Event;
            VK_UI3.Views.Tasks.TaskAction.tasks.CollectionChanged += Tasks_CollectionChanged;
            VK_UI3.Views.Notification.Notification.Notifications.CollectionChanged += Notifications_CollectionChanged;
            UploadTrack.UploadsTracks.CollectionChanged += UploadsTracks_CollectionChanged;

            AnimatedButton.AnimationCompleted += ResizeTabBar;
            DownLoadBTN.AnimationCompleted += ResizeTabBar;
            BackBTN.AnimationCompleted += ResizeTabBar;
            ProfilesBTN.AnimationCompleted += ResizeTabBar;
            UploadBTN.AnimationCompleted += ResizeTabBar;

        }
        // Win32 API функции
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);


        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // Константы стилей
        private const int GWL_EXSTYLE = -20;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;

        private TrayIconManager _trayIconManager = null;
        public void HideFromTaskbar()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            if (hwnd == IntPtr.Zero)
                return;

            var extendedStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
            SetWindowLongPtr(hwnd, GWL_EXSTYLE,
                (IntPtr)(extendedStyle.ToInt64() | WS_EX_TOOLWINDOW));

            ShowWindow(hwnd, 0);
        }
        private const int WS_EX_APPWINDOW = 0x00040000;

        public void ShowWindowAgain()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            if (hwnd == IntPtr.Zero)
                return;

            // Получаем текущие расширенные стили
            IntPtr extendedStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);

            // Убираем флаг WS_EX_TOOLWINDOW
            extendedStyle = (IntPtr)(extendedStyle.ToInt64() & ~WS_EX_TOOLWINDOW);

            // Опционально: добавляем WS_EX_APPWINDOW для явного указания на показ в панели задач
            extendedStyle = (IntPtr)(extendedStyle.ToInt64() | WS_EX_APPWINDOW);

            // Устанавливаем обновлённые стили
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, extendedStyle);

            var hWnd = WindowNative.GetWindowHandle(this);
            ShowWindow(hWnd, 5); // Показать окно
            _trayIconManager?.HideTrayIcon();
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

        private void ResizeTabBar()
        {
            SetRegionsForCustomTitleBar();
        }

        private void InitializeUIComponents()
        {
            VK.api.RequestsPerSecond = 50;
            CheckMica();
            updateMica += OnUpdateMica;

            AppWindowTitleBar m_TitleBar = m_AppWindow.TitleBar;
            m_TitleBar.ButtonForegroundColor = (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                ? Colors.White : Colors.Black;

            UISettings uI = new();
            uI.ColorValuesChanged += UI_ColorValuesChanged;
        }

        private void InitializeSystemIntegration()
        {
            
            dispatcherQueue = this.DispatcherQueue;
            SubClassing();
            string assetsPath;
            try
            {
                 assetsPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            }
            catch (InvalidOperationException)
            {
                assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
            }
            string iconPath = Path.Combine(assetsPath, "icon.ico");
            LoadIcon(iconPath);

            hvn = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);
        }

        private void InitializeNavigation()
        {
            if (SettingsTable.GetSetting("createShortcut") is null)
            {
               _= ShortcutManager.CreateDesktopShortcutAsync();
                SettingsTable.SetSetting("createShortcut", "1");

                if (!Packaged.IsPackaged())
                {
                   _= ShortcutManager.CreateStartMenuShortcutAsync();
                }
            }

            var navigationInfo = new NavigationInfo { SourcePageType = this };
            if (AccountsDB.GetAllAccounts().Count == 0)
            {
                GoLogin();
            }
            else
            {
                ContentFrame.Navigate(typeof(MainView), navigationInfo, new DrillInNavigationTransitionInfo());
            }
        }

        #endregion

        #region Window Management
        private void MainWindow_CloseRequested(object sender, WindowEventArgs args)
        {
            // Если включено скрытие в трей, сразу прячем окно и не проверяем активные загрузки.
            var hideTray = DB.SettingsTable.GetSetting("hideTray");
            if (hideTray == null)
            {
                args.Handled = true;

                showDialogTrayAsync();
                return;
            }



        
            // Если принудительное закрытие (justClose) – завершаем приложение.
            if (justClose)
            {
                Application.Current.Exit();
                return;
            }
            if (hideTray.settingValue.Equals("1"))
            {
                args.Handled = true;
                var hwnd = WindowNative.GetWindowHandle(this);
                if (_trayIconManager == null)
                {
                    _trayIconManager = new TrayIconManager(hwnd, this);
                } else
                {
                    _trayIconManager.ShowTrayIconAgain();
                }
                HideFromTaskbar();
                return;
            }
            // Если есть активные загрузки – предлагаем пользователю варианты.
            if (PlayListDownload.PlayListDownloads.Count != 0)
            {
                args.Handled = true;
                HandleCloseWithActiveDownloads();
                return;
            }

            // Если нет никаких особенностей – закрываем приложение.
            Application.Current.Exit();
        }

        private async Task showDialogTrayAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "Спрятать окно в трей?",
                Content = "Укажите, как желаете, чтобы вело себя окно при закрытии?",
                PrimaryButtonText = "Закрывать",
                SecondaryButtonText = "Прятать в трей",
                CloseButtonText = "Отмена"
            };

            dialog.Resources["ContentDialogMaxWidth"] = double.PositiveInfinity;
            dialog.XamlRoot = this.Content.XamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                DB.SettingsTable.SetSetting("hideTray", "0");
                Application.Current.Exit();
            }
            else if (result == ContentDialogResult.Secondary)
            {

                DB.SettingsTable.SetSetting("hideTray", "1");
                Application.Current.Exit();

            }
        }

        private async void HandleCloseWithActiveDownloads()
        {
            var dialog = new ContentDialog
            {
                Title = "Загрузка еще не завершена",
                Content = "Вы уверены, что хотите закрыть приложение, несмотря на активные загрузки?",
                PrimaryButtonText = "Да, закрыть",
                SecondaryButtonText = "Закрыть по завершению",
                CloseButtonText = "Отмена"
            };

            dialog.Resources["ContentDialogMaxWidth"] = double.PositiveInfinity;
            dialog.XamlRoot = this.Content.XamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Закрываем приложение немедленно.
                justClose = true;
                Application.Current.Exit();
            }
            else if (result == ContentDialogResult.Secondary && !addClosed)
            {
                // Подписываемся на событие завершения всех загрузок, чтобы закрыть приложение после их окончания.
                addClosed = true;
                PlayListDownload.OnEndAllDownload += close;
            }
            // При выборе "Отмена" – ничего не делаем, окно остаётся открытым.
        }


        private void close(object sender, EventArgs e) => Application.Current.Exit();
        #endregion

        #region UI Controls Visibility
        public void backBTNShow() => BackBTN.ShowButton();
        public void backBTNHide() => BackBTN.HideButton();

        public void MainWindow_showRefresh()
        {
            AnimatedButton.ShowButton();
            UpdateTitleBarRegions();
        }

        public void MainWindow_hideRefresh()
        {
            AnimatedButton.HideButton();
            UpdateTitleBarRegions();
        }

        public void MainWindow_showDownload()
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                DownLoadBTN.ShowButton();
                DownLoadBTN.Flyout.ShowAt(DownLoadBTN);
                UpdateTitleBarRegions();
            });
        }

        public void MainWindow_hideDownload()
        {
            DownLoadBTN.HideButton();
            UpdateTitleBarRegions();
        }
        #endregion

        #region Title Bar and Regions Management
        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (ExtendsContentIntoTitleBar)
            {
                MainWindow_hideDownload();
                MainWindow_hideRefresh();
                backBTNHide();
                UploadBTN.HideButton();
                UpdateTitleBarRegions();
            }
#if !DEBUG
            checkUpdate();
#endif
            checkNotifications();
            InitializeSnow();
        }

        private async void checkNotifications()
        {
            try
            {
                var not = new Helpers.NotificationsGetter();
                var notifs = await not.GetNotificationsAsync();


                foreach (var item in notifs)
                {
                    try
                    {
                        ButtonNotification button1 = null;
                        ButtonNotification button2 = null;

                        // Создаем первую кнопку, если есть ссылки
                        if (item.Links != null && item.Links.Count > 0)
                        {
                            button1 = CreateButtonNotification(item.Links[0]);
                        }

                        // Создаем вторую кнопку, если есть минимум 2 ссылки
                        if (item.Links != null && item.Links.Count > 1)
                        {
                            button2 = CreateButtonNotification(item.Links[1]);
                        }

                        new Notification(
                            item.Header,
                            item.Message,
                            button1,
                            button2
                        );
                    }
                    catch { }
                }
            }
            catch { 
            }

        }

        private ButtonNotification CreateButtonNotification(NotificationLink link)
        {
            return new ButtonNotification(
                link.Name,
                new Action(() =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = link.Url
                    });
                }),
                true
            );
        }


        #region Snow
        private void InitializeSnow()
        {
            var enabledSnowSetting = SettingsTable.GetSetting("snowflakesEnabled");

            if (enabledSnowSetting == null)
            {
                InitializeSnowBySeason();
                return;
            }

            if (enabledSnowSetting.settingValue == "1")
            {
                snow.Start();
                SetFlakeCount();
            }
            else
            {
                snow.Stop();
            }

            
        }

        private void InitializeSnowBySeason()
        {
            int currentMonth = DateTime.Now.Month;
            bool isWinter = currentMonth == 12 || currentMonth == 1 || currentMonth == 2;

            if (isWinter)
            {
                snow.Start();
                SetFlakeCount();
            }
        }

        private void SetFlakeCount()
        {
            var countSetting = SettingsTable.GetSetting("snowflakesCount");
            snow.FlakeCount = countSetting == null
                ? 100
                : ParseSnowflakeCount(countSetting.settingValue);
        }

        private int ParseSnowflakeCount(string value)
        {
            if (int.TryParse(value, out int count) && count > 0)
            {
                return count;
            }

            // Значение по умолчанию при некорректных данных
            return 100;
        }

        public VK_UI3.SnowFlake.SnowFlakeEffect Snow { get { return snow; }  }

        #endregion

        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ExtendsContentIntoTitleBar)
                UpdateTitleBarRegions();
        }

        private void UpdateTitleBarRegions()
        {
            if (AppTitleBar.XamlRoot == null)
                return;

            double scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;
            var nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(m_AppWindow.Id);
            var children = AppTitleBar.Children;
            var rects = new List<Windows.Graphics.RectInt32>();

            foreach (var child in children)
            {
                if (child is not FrameworkElement frameworkElement)
                    continue;
                if (frameworkElement.Name == "TitIcon" || frameworkElement.Name == "AppTitle")
                    continue;

                var transform = child.TransformToVisual(null);
                var bounds = transform.TransformBounds(new Rect(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight));

                rects.Add(new Windows.Graphics.RectInt32(
                    _X: (int)Math.Round(bounds.X * scaleAdjustment),
                    _Y: (int)Math.Round(bounds.Y * scaleAdjustment),
                    _Width: (int)Math.Round(bounds.Width * scaleAdjustment),
                    _Height: (int)Math.Round(bounds.Height * scaleAdjustment)
                ));
            }

            nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rects.ToArray());
        }
        #endregion

        #region Event Handlers
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e) => backBTNHide();

        private void OnEndAllDownload_Event(object sender, EventArgs e)
        {
            DownLoadBTN.Flyout.Hide();
            MainWindow_hideDownload();
        }

        private void OnUpdateMica(object sender, EventArgs e) => CheckMica();

        private void UI_ColorValuesChanged(UISettings sender, object args)
        {
            DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
            {
                AppWindowTitleBar m_TitleBar = m_AppWindow.TitleBar;
                m_TitleBar.ButtonForegroundColor = (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                    ? Colors.White : Colors.Black;
            });
        }

        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (VK_UI3.Views.Tasks.TaskAction.tasks.Count == 0)
            {
                TasksBTN.HideButton();
                TasksBTN.Flyout.Hide();
            }
            else
            {
                TasksBTN.ShowButton();
                TasksBTN.Flyout.ShowAt(TasksBTN);
            }
        }

        private void UploadsTracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<UploadTrack> traks)
            {
                if (traks.Count > 0)
                {
                    UploadBTN.ShowButton();
                    UploadBTN.Flyout.ShowAt(UploadBTN);
                }
                else
                {
                    UploadBTN.HideButton();
                    UploadBTN.Flyout.Hide();
                }
            }
        }

        private void Notifications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (VK_UI3.Views.Notification.Notification.Notifications.Count == 0)
            {
                NotifList.Hide();
                NotifIndicator.Visibility = Visibility.Collapsed;
            }
            else
            {
                NotifList.ShowAt(TitIcon);
                NotifIndicator.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Navigation
        public void GoLogin()
        {
            ContentFrame.Navigate(typeof(Login), this, new DrillInNavigationTransitionInfo());
        }
        #endregion

        #region System Integration
        private void LoadIcon(string aIconName)
        {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, aIconName);
            var hwnd = new HWND(WinRT.Interop.WindowNative.GetWindowHandle(this));
            iIcon = PInvoke.LoadImage(null, iconPath, GDI_IMAGE_TYPE.IMAGE_ICON, 128, 128, IMAGE_FLAGS.LR_LOADFROMFILE);
            PInvoke.SendMessage(hwnd, PInvoke.WM_SETICON, new WPARAM(0), new LPARAM(iIcon.DangerousGetHandle()));
        }

        private void SubClassing()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            if (hwnd == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to get window handler: error code {error}");
            }

            newWndProc = new(NewWindowProc);
            oldWndProc = SetWindowLong((HWND)hwnd, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(newWndProc));

            if (oldWndProc == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to set GWL_WNDPROC: error code {error}");
            }
        }

        private IntPtr NewWindowProc(HWND hWnd, uint Msg, WPARAM wParam, IntPtr lParam)
        {
            if (Msg == WM_GETMINMAXINFO)
            {
                var dpi = PInvoke.GetDpiForWindow(hWnd);
                float scalingFactor = (float)dpi / 96;

                var minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                minMaxInfo.ptMinTrackSize.x = (int)(700 * scalingFactor);
                minMaxInfo.ptMinTrackSize.y = (int)(500 * scalingFactor);
                Marshal.StructureToPtr(minMaxInfo, lParam, true);
            }

            WNDPROC oldWndProcDelegate = (WNDPROC)Marshal.GetDelegateForFunctionPointer(oldWndProc, typeof(WNDPROC));
            return PInvoke.CallWindowProc(oldWndProcDelegate, hWnd, Msg, wParam, lParam);
        }

        private static IntPtr SetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, IntPtr newProc)
        {
            if (Environment.Is64BitOperatingSystem)
            {
#if X64
                return PInvoke.SetWindowLongPtr(hWnd, nIndex, newProc);
#endif
            }
            return new IntPtr(PInvoke.SetWindowLong(hWnd, nIndex, newProc.ToInt32()));
        }
        #endregion

        #region Background and Theme
        private void CheckMica()
        {
            var set = SettingsTable.GetSetting("backDrop");
            if (set != null)
            {
                if (!(SystemBackdrop is MicaBackdrop))
                    SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt };
            }
            else if (!(SystemBackdrop is DesktopAcrylicBackdrop))
            {
                SystemBackdrop = new DesktopAcrylicBackdrop();
            }
        }
        #endregion

        #region Button Handlers
        public void ShowTaskFlyOut() => TasksBTN.Flyout.ShowAt(TasksBTN);

        public static void onRefreshClickedvoid()
        {
            MainWindow.mainWindow.RotationStoryboard.Begin();
            onRefreshClicked?.Invoke(null, EventArgs.Empty);
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            ScaleStoryboardBackBTN.Begin();
            onBackClicked?.Invoke(this, EventArgs.Empty);
        }

        private void RefreshClick_Click(object sender, RoutedEventArgs e)
        {
            RotationStoryboard.Begin();
            onRefreshClicked?.Invoke(this, EventArgs.Empty);
        }

        private void DownLoadBTN_Click(object sender, RoutedEventArgs e)
        {
            onDownloadClicked?.RaiseEvent(this, EventArgs.Empty);
        }

        private void TitIcon_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (VK_UI3.Views.Notification.Notification.Notifications.Count > 0)
                NotifList.ShowAt(TitIcon);
        }
        #endregion

        #region Updates and Downloads
        public async Task<bool> checkUpdate()
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var currentVersion = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}.{assemblyVersion.Revision}";
            var appUpdater = new AppUpdater(currentVersion);
            appUpdater.SelectedPackageType = Packaged.IsPackaged() ? PackageType.MSIX : PackageType.ZIP;
            var updateAvailable = await appUpdater.CheckForUpdates();

            if (updateAvailable)
            {
                if (Packaged.IsPackaged())
                {
                    if (!appUpdater._currentReleaseInfo.Assets.ContainsKey(PackageType.MSIX))
                    {
                        return false;

                    }
                }
                else
                {
                    if (!appUpdater._currentReleaseInfo.Assets.ContainsKey(PackageType.ZIP))
                    {
                        return false;
                    }
                }


                dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
                    {
                        new Notification(new UpdatePage(appUpdater));
                        //ContentFrame.Navigate(typeof(UpdatePage), appUpdater, new DrillInNavigationTransitionInfo());
                    });
                return true;
            }
            return false;
        }

        internal async Task requstDownloadFFMpegAsync()
        {
            try
            {
                if (downloadFileWithProgress == null)
                {
                    downloadFileWithProgress = new DownloadFileWithProgress();
                }
                downloadFileWithProgress?.DownloadFile();
                MainWindow_showDownload();
            }
            catch (Exception e)
            {
                // Обработка ошибок
            }
        }

        internal SystemMediaTransportControls getCurrentView()
        {
            return SystemMediaTransportControls.GetForCurrentView();
        }
        #endregion

        #region Structs for Window Processing
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }
        #endregion
    }
}