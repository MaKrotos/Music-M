using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MusicX.Core.Services;
using MusicX.Services;
using NLog;
using System;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Services;
using VK_UI3.VKs.Ext;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.Extensions.DependencyInjection;

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







            //   await (appUpdater.CheckForUpdaterBool)
        }

        public static Microsoft.UI.Xaml.Window m_window;
    }
}
