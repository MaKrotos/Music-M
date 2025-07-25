﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Sources;
using NLog;
using StatSlyLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.DownloadTrack;
using VK_UI3.VKs.Ext;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.Extensions.DependencyInjection;
using Windows.ApplicationModel;
using Windows.Win32;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {


        public static readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(c =>
        {
            c.SetBasePath(AppContext.BaseDirectory);
        })
        .ConfigureServices(services =>
        {
            // Vk Net services
            // services.AddVkNet();



            // services.AddVkNetWithAuth();
            services.AddAudioBypass();
            services.AddVkNet();
            // services.AddSingleton<IAsyncCaptchaSolver, CaptchaSolverService>();
            // Vk Net implementations
            services.AddSingleton<IVkTokenStore, RegistryTokenStore>();
            services.AddSingleton<IDeviceIdStore, RegistryTokenStore>();
            services.AddSingleton<IExchangeTokenStore, RegistryTokenStore>();
            services.AddSingleton(LogManager.Setup().GetLogger("Common"));
            services.AddSingleton<IAsyncCaptchaSolver, CaptchaSolverService>();
            services.AddSingleton<TokenChecker>();

            services.AddSingleton<VkService>();
            services.AddSingleton<ListenTogetherService>();
            services.AddSingleton<UserRadioService>();
            services.AddSingleton<BoomService>();
            services.AddSingleton<ITrackMediaSource, VkMediaSource>();

            services.AddSingleton<GeniusService>();

            services.AddTransient<VkBridgeService>();

            FFMediaToolkit.FFmpegLoader.FFmpegPath = new CheckFFmpeg().GetFFmpegDirectory() + "\\";

            services.AddSingleton<ICustomSectionsService, CustomSectionsService>();

            var container = StaticService.Container = services.BuildServiceProvider();


            // var container  = services.BuildServiceProvider();
            if (AccountsDB.activeAccount.Token == null)
            {
                Task.Run(
                    async () =>
                    {
                        await container.GetRequiredService<IVkApiAuthAsync>()
                                          .AuthorizeAsync(new AndroidApiAuthParams());
                    }
             );
            }

            //services.AddHostedService<ApplicationHostService>();
        })
        .Build();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            thisApp = this;
            Application.Current.RequestedTheme = ApplicationTheme.Dark;
        }


        private Mutex _mutex = null;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            statSlyRun();

            const string mutexName = "VKMMaKrotosApps";
            bool createdNew;

         
            setThemeApp();
            
                _mutex = null;
                Process current = Process.GetCurrentProcess();
            bool close = false;

            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
            {
                Windows.Win32.Foundation.BOOL EnumWindowCallback(Windows.Win32.Foundation.HWND hwnd, Windows.Win32.Foundation.LPARAM lParam)
                {
                    uint processId;
                    GetWindowThreadProcessId(hwnd, out processId);

                    if (processId == process.Id)
                    {
                        // Получаем заголовок окна
                        int length = PInvoke.GetWindowTextLength(hwnd);
                        if (length > 0)
                        {
                            // Создаем буфер достаточного размера (+1 для нуль-терминатора)
                            char[] buffer = new char[length + 1];
                            unsafe
                            {
                                fixed (char* pBuffer = buffer)
                                {
                                    // Получаем текст окна
                                    int copiedChars = PInvoke.GetWindowText(hwnd, (Windows.Win32.Foundation.PWSTR)pBuffer, buffer.Length);
                                    if (copiedChars > 0)
                                    {
                                        string windowTitle = new string(buffer, 0, copiedChars);

                                        // Проверяем, содержит ли заголовок "VK M" (или точное совпадение)
                                        if (windowTitle.Contains("VK M")) // или windowTitle == "VK M"
                                        {
                                            if (PInvoke.IsWindowVisible(hwnd))
                                            {
                                                if (PInvoke.IsIconic(hwnd))
                                                {
                                                    PInvoke.ShowWindow(hwnd, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE);
                                                }
                                                PInvoke.SetForegroundWindow(hwnd);
                                            }
                                            else
                                            {
                                                PInvoke.ShowWindow(hwnd, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_SHOWDEFAULT);
                                                PInvoke.SetForegroundWindow(hwnd);
                                            }
                                            close = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return true;
                }

                // Перебираем все окна
                PInvoke.EnumWindows(EnumWindowCallback, (Windows.Win32.Foundation.LPARAM)0);
            }
            if (close)
            {
                Application.Current.Exit();
                return;
            }

            _host.Start();

            m_window = new MainWindow();
            m_window.Closed += M_window_Closed;

            m_window.Activate();

            this.UnhandledException += App_UnhandledException;

            //   await (appUpdater.CheckForUpdaterBool)
        }

        private async Task statSlyRun()
        {
            try
            {
             

                var setting = DB.SettingsTable.GetSetting("UserUniqID");
                string UserUniqID;
                var packageVersion = Package.Current.Id.Version;
                var version = $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
                if (setting == null)
                {
                    UserUniqID = Helpers.SmallHelpers.GenerateRandomString(100);
                    DB.SettingsTable.SetSetting("UserUniqID", UserUniqID);

                    EventParams eventParams = new EventParams("userID", UserUniqID);

                    Event @event = new Event("First Run", DateTime.Now, eventParams: new List<EventParams>() {
                        new EventParams("userID", UserUniqID),
                        new EventParams("versionAPP", version),

                        new EventParams("OSArchitecture", RuntimeInformation.OSArchitecture.ToString()),
                        new EventParams("AppArchitecture", RuntimeInformation.ProcessArchitecture.ToString())
                    });

                    _ = new VKMStatSly().SendEvent(@event);
                }
                else
                {
                    UserUniqID = setting.settingValue;
                }

                {
                 
               

                    var listParams = new List<EventParams>
                    {
                        new EventParams("userID", UserUniqID),
                        new EventParams("Accounts Count", AccountsDB.GetAllAccounts().Count),
                        new EventParams("versionAPP", version),
                        new EventParams("OSArchitecture", RuntimeInformation.OSArchitecture.ToString()),
                        new EventParams("AppArchitecture", RuntimeInformation.ProcessArchitecture.ToString())
                    };

                    if (AccountsDB.GetAllAccounts().Count > 0)
                    {
                        var account = AccountsDB.GetActiveAccount();
                        listParams.Add(new EventParams("ActiveAccount", account.GetHash()));
                    }

                    Event @event = new Event("Run App", DateTime.Now, eventParams: listParams);
                    _ = new VKMStatSly().SendEvent(@event);


                    setting = DB.SettingsTable.GetSetting("FirstRunDate");
                 

                    DateOnly nowDate = DateOnly.FromDateTime(DateTime.Now);

                    if (setting != null &&
                        DateOnly.Parse(setting.settingValue) == nowDate)
                    {
                        return;
                    }

                    @event = new Event("FirstRunDay", DateTime.Now, eventParams: listParams);

                    try
                    {
                        await (new VKMStatSly().SendEvent(@event));
                        DB.SettingsTable.SetSetting("FirstRunDate", nowDate.ToString());
                    }
                    catch 
                    {

                    }

                }
            }
            catch (Exception e)
            { 
            }
        }

        private static App thisApp;

        public static void setThemeApp() 
        {
            //thisApp.RequestedTheme = ApplicationTheme.Light;

            //thisApp.RequestedTheme = (DB.SettingsTable.GetSetting("SetLightColor") == null) ? ApplicationTheme.Light : ApplicationTheme.Dark;
        }

        private void M_window_Closed(object sender, WindowEventArgs args)
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex = null;
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex = null;
                return;
            }

            var setting = DB.SettingsTable.GetSetting("UserUniqID");
            string UserUniqID;
            if (setting == null)
            {
                UserUniqID = Helpers.SmallHelpers.GenerateRandomString(100);
                DB.SettingsTable.SetSetting("UserUniqID", UserUniqID);



            }
            else
            {
                UserUniqID = setting.settingValue;
            }


            var packageVersion = Package.Current.Id.Version;
            var version = $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";


            var listParams = new List<EventParams>
                    {
                        new EventParams("userID", UserUniqID),
                        new EventParams("Accounts Count", AccountsDB.GetAllAccounts().Count),
                        new EventParams("versionAPP", version),
                        new EventParams("Sender", sender.ToString()),
                        new EventParams("Exception", e.Message),
                        new EventParams("StackTrace", e.Exception.StackTrace),
                        new EventParams("OSArchitecture", RuntimeInformation.OSArchitecture.ToString()),
                        new EventParams("AppArchitecture", RuntimeInformation.ProcessArchitecture.ToString())
                    };

            if (AccountsDB.GetAllAccounts().Count > 0)
            {
                var account = AccountsDB.GetActiveAccount();
                listParams.Add(new EventParams("ActiveAccount", account.GetHash()));
            }

            Event @event = new Event("Exception", DateTime.Now, eventParams: listParams);
            _ = new VKMStatSly().SendEvent(@event);
        }

      
        public static Microsoft.UI.Xaml.Window m_window;
    }
}
