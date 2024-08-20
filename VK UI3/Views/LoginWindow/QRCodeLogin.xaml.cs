using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using VK_UI3.Helpers.Animations;
using VK_UI3.VKs;
using VkNet.AudioBypassService.Models.Auth;
using System.Timers;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using SQLitePCL;
using VK_UI3.DB;
using VkNet.Enums.Filters;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.LoginWindow
{
    public sealed partial class QRCodeLogin : Page
    {
        public QRCodeLogin()
        {
            this.InitializeComponent();
            this.Loaded += QRCodeLogin_Loaded;
            AnimationsChangeImage = new Helpers.Animations.AnimationsChangeImage(qrcodeimage, DispatcherQueue);

            Timer _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Interval = 10000;
            _timer.Start();
 

    }

        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (authcode != null) {
                var checkCode = await new VK()._authCategory.CheckAuthCodeAsync(authcode.AuthHash, authcode.Token);
                switch (checkCode.Status)
                {
                    case Statuses.Continue:
                        break;
                    case Statuses.ConfirmOnPhone:
                        
                        break;
                    case Statuses.Ok:
                        LoggedInAs(checkCode);
                        AnimationsChangeImage.ChangeImageWithAnimation("null");
                        break;
                    case Statuses.Expired:
                        ChangeQR();
                        break;
                    case Statuses.Loading:
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task LoggedInAs(AuthCheckResponse checkCode)
        {

            try
            {


                var vk = new VK();
                var a = await vk._authCategory.connect_code_auth(checkCode.super_app_token, Guid.NewGuid().ToString());

                AccountsDB.activeAccount.Token = a;
          

                AccountsDB.activeAccount.ExchangeToken = a;

                var profile = (await VK.api.Users.GetAsync(new List<long> { }, ProfileFields.PhotoMax)).FirstOrDefault();


                AccountsDB.activeAccount.id = profile.Id;
                AccountsDB.activeAccount.Name = $"{profile.FirstName} {profile.LastName}";


                AccountsDB.activeAccount.UserPhoto = (profile.PhotoMax ?? profile.Photo400Orig ?? profile.Photo200Orig ?? profile.Photo200 ?? profile.Photo100 ?? new Uri("https://vk.com/images/camera_200.png")).ToString();

                AccountsDB.activeAccount.Update();

                DispatcherQueue.TryEnqueue(async () =>
                {
                    Frame.Navigate(typeof(MainView), null, new DrillInNavigationTransitionInfo());
                });
            }
            catch (Exception ex)
            {
                AnimationsChangeImage.ChangeImageWithAnimation("null");
            }
        }

        private void QRCodeLogin_Loaded(object sender, RoutedEventArgs e)
        {
         
            Task.Run(async () => await ChangeQR());
            if (Frame.CanGoBack)
                BackBTN.Visibility = Visibility.Visible;
            else
                BackBTN.Visibility = Visibility.Collapsed;
        }
        AuthCodeResponse authcode = null;
        Helpers.Animations.AnimationsChangeImage AnimationsChangeImage { get; set; }
        private async Task ChangeQR()
        {
            authcode = await new VK().LoadQrCode(DispatcherQueue);
            BitmapImage a = authcode.image as BitmapImage;

            AnimationsChangeImage.ChangeImageWithAnimation(a);

            // Set up the timer to call ChangeQR again after ExpiresIn seconds
            DateTime expiresAt = DateTimeOffset.FromUnixTimeSeconds(authcode.ExpiresIn).DateTime;

            TimeSpan timeUntilExpiry = expiresAt - DateTime.Now;
            if (_timer == null)
            {
                _timer = new Timer(); // Convert TimeSpan to milliseconds


                _timer.Elapsed += async (sender, e) => await OnTimedEvent();
                _timer.AutoReset = false;
            }

            _timer.Interval = timeUntilExpiry.TotalMilliseconds;
            _timer.Stop();
            _timer.Start();
        }

        private async Task OnTimedEvent()
        {
            AnimationsChangeImage.ChangeImageWithAnimation("null");
            _timer.Stop();
            await ChangeQR();
        }
        private Timer _timer;

        private void BackBTN_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void qrcodeimage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (authcode != null)
            {
                if (e.GetCurrentPoint(qrcodeimage).Properties.IsRightButtonPressed)
                {
                    var dataPackage = new DataPackage();
                    dataPackage.SetText(authcode.AuthUrl);
                    Clipboard.SetContent(dataPackage);
                }
                else
                {
                    Process.Start(new ProcessStartInfo(authcode.AuthUrl) { UseShellExecute = true });
                }
            }
        }

    }
}
