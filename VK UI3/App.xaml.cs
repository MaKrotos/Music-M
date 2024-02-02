using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Extensions;

using VkNet.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using VK_UI3.DB;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Models.Auth;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using System.Threading.Tasks;
using VK_UI3.VKs.Ext;
using System.Runtime.InteropServices;
using Windows.Win32;
using WinRT.Interop;
using Windows.Win32.Foundation;
using MusicX.Core.Services;
using NLog;
using VK_UI3.Services;
using MusicX.Services;

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
        }

       


        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _host.Start();
           

            m_window = new MainWindow();

       
            m_window.Activate();


            AppCenter.Start("4f42c41a-220a-497f-9b87-f2d601a6d674",
                  typeof(Analytics), typeof(Crashes));

            // Set the window procedure
            // Check if the OS is 64-bit




            //   await (appUpdater.CheckForUpdaterBool)
        }

        private Microsoft.UI.Xaml.Window m_window;
    }
}
