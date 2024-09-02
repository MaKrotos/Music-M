using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MusicX.Core.Services;
using MusicX.Services;
using NLog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Services;
using VK_UI3.VKs.Ext;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.Extensions.DependencyInjection;
using Windows.ApplicationModel.Activation;
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

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            const string mutexName = "VKMMaKrotosApp";
            bool createdNew;
            
            setThemeApp();
            _mutex = new Mutex(true, mutexName, out createdNew);
            if (!createdNew)
            {
                // Здесь ваш код для запуска приложения
                _mutex = null;

                // Получаем текущий процесс
                Process current = Process.GetCurrentProcess();

                // Получаем все процессы с таким же именем, как у текущего
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                {
                    // Если процесс не является текущим и его главное окно не минимизировано
                    if (process.Id != current.Id && process.MainWindowHandle != IntPtr.Zero)
                    {
                        // Развертываем окно, если оно свернуто
                        var a = new Windows.Win32.Foundation.HWND(process.MainWindowHandle);
                        if (PInvoke.IsIconic(new Windows.Win32.Foundation.HWND(a)))
                        {
                            PInvoke.ShowWindow(new Windows.Win32.Foundation.HWND(a), Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE);
                        }

                        // Переводим окно на передний план
                        PInvoke.SetForegroundWindow(new Windows.Win32.Foundation.HWND(a));
                        break;
                    }
                }
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
            }
        }

      
        public static Microsoft.UI.Xaml.Window m_window;
    }
}
