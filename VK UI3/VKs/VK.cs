using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VkNet.AudioBypassService.Extensions;
using VkNet;
using System.Threading.Tasks;
using VkNet.Model;
using Microsoft.Extensions.DependencyInjection;
using VK_UI3.Views.LoginFrames;
using VkNet.Infrastructure.Authorization.ImplicitFlow.Forms;
using static VK_UI3.DB.AccountsDB;
using VkNet.Enums.Filters;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Media.Animation;
using VK_UI3.Views;
using VkNet.Utils;
using Windows.Devices.Radios;
using VkNet.Enums.StringEnums;
using Microsoft.UI.Dispatching;

namespace VK_UI3.VKs
{
    internal class VK
    {





        internal static async Task LoginAsync(string text, string password, Login login, string code = null)
        {

            var services = new ServiceCollection();
            services.AddAudioBypass();

            var api = new VkApi(services);

            var loginParams = new ApiAuthParams
            {
                Login = text,
                Password = password,
            };

            if (code != null)
            {



                loginParams.Code = code;
                loginParams.ForceSms = true;
            }


            try
            {
                api.Authorize(loginParams);
            }
            catch (VkNet.AudioBypassService.Exceptions.VkAuthException e)
            {
                login.DialogMessageShow(e.Message);
            }
            catch (VkNet.Exception.CaptchaNeededException cne)
            {


                login.InputTextDialogAsyncCapthca(cne.Img);
            }
            catch (Exception ex)
            {

                if (ex.Message == "Two-factor authorization required, but TwoFactorAuthorization callback is null. Set TwoFactorAuthorization callback to handle two-factor authorization.")
                {


                    LoginAsync(text, password, login, await login.InputTextDialogAsync());
                }
                else
                {

                }
            }

            if (api.IsAuthorized)
            {
                Accounts accounts = new Accounts();
                accounts.Token = api.Token;

                var user = api.Users.Get(new List<long> { api.UserId.Value }, ProfileFields.PhotoMax).FirstOrDefault();

                var account = new Accounts
                {
                    id = user.Id,
                    Active = true,
                    Name = $"{user.FirstName} {user.LastName}",
                    Token = api.Token,
                    UserPhoto = user.PhotoMax.ToString(),
                };
                var accountsList = GetAccByID(user.Id);
                if (accountsList.Count > 0)
                {
                    DatabaseHandler.getConnect().Update(account);
                }
                else DatabaseHandler.getConnect().Insert(account);

                ActivateAccount(user.Id);

                login.Frame.Navigate(typeof(MainView), null, new DrillInNavigationTransitionInfo());
            }

        }

        public async Task<string> GetAlbumCover(long? ownerId, long albumId)
        {

            // Убедитесь, что api не является null
            var services = new ServiceCollection();
            services.AddAudioBypass();

            var api = new VkApi(services);

            var actAcc = GetActiveAccounts()[0];

            api.Authorize(new ApiAuthParams
            {
                AccessToken = actAcc.Token
            });

            var audio = api.Audio.Get(new AudioGetParams
            {
                OwnerId = ownerId,
                AlbumId = albumId
            });

            var track = audio.FirstOrDefault();

            if (track != null && track.Album != null)
            {
                return track.Album.Thumb.Photo300;
            }

            return null;
        }

        public static VkApi getVKAPI()
        {
            // Убедитесь, что api не является null
            var services = new ServiceCollection();
            services.AddAudioBypass();

            var api = new VkApi(services);

            var actAcc = GetActiveAccounts()[0];
            api.Authorize(new ApiAuthParams
            {
                AccessToken = actAcc.Token
            });

            if (!api.IsAuthorized)
            {
                DatabaseHandler.getConnect().Delete(actAcc);
            }
            api.UserId = actAcc.id;

            return api;
        }

        public static List<Audio> GetUserMusic()
        {
            // Убедитесь, что api не является null
            var services = new ServiceCollection();
            services.AddAudioBypass();

            var api = new VkApi(services);

            var actAcc = GetActiveAccounts()[0];



            api.Authorize(new ApiAuthParams
            {
                AccessToken = actAcc.Token
            });

            if (api == null)
            {
                // Обработайте ситуацию, когда api равно null, выбросьте исключение или верните значение по умолчанию
                throw new InvalidOperationException("API не инициализирован.");
            }
            var isact = api.IsAuthorized;

            // Убедитесь, что результат Audio.Search не является null
            VkCollection<Audio> audio;
            try
            {
                var audios = api.Audio.Get(new AudioGetParams
                {
                    OwnerId = actAcc.id
                });
                return audios.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }


    }
}
