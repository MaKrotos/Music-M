using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using MusicX.Core.Services;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Helpers;
using VK_UI3.Views;
using VK_UI3.Views.LoginWindow;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Abstractions.Categories;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Models.Ecosystem;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using Windows.Storage.Streams;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WindowsWebServices;
using IAuthCategory = VkNet.AudioBypassService.Abstractions.Categories.IAuthCategory;

namespace VK_UI3.VKs
{
    internal class VK
    {
        public static readonly IVkApi api = App._host.Services.GetRequiredService<IVkApi>();
        public readonly IAuthCategory _authCategory = App._host.Services.GetRequiredService<IAuthCategory>();
        Login login;
        private readonly IEcosystemCategory _ecosystemCategory = App._host.Services.GetRequiredService<IEcosystemCategory>();
        private TaskCompletionSource<string>? _codeTask;
        private readonly IVkApiAuthAsync _vkApiAuth = App._host.Services.GetRequiredService<IVkApiAuthAsync>();
        public readonly IVkApiAuthAsync _vkApi = App._host.Services.GetRequiredService<IVkApiAuthAsync>();
        public static readonly VkService vkService = App._host.Services.GetRequiredService<VkService>();


        public WeakEventManager LoggedIn = new WeakEventManager();



        IVkApiAuthAsync vkApi = App._host.Services.GetRequiredService<IVkApiAuthAsync>();

        public TokenChecker checker = App._host.Services.GetRequiredService<TokenChecker>();

        public static readonly BoomService boomService = App._host.Services.GetRequiredService<BoomService>();


        public static readonly SafeBoomService safeBoomService = new SafeBoomService(boomService);


        public class SafeBoomService
        {
            private readonly BoomService _boomService;

            public SafeBoomService(BoomService boomService)
            {
                _boomService = boomService;

            }

            public T CallWithRetry<T>(Func<BoomService, T> func)
            {
                int retryCount = 0;
                while (retryCount < 5)
                {
                    try
                    {
                        return func(_boomService);
                    }
                    catch (Exception ex)
                    {

                        AuthBoomAsync();
                        Console.WriteLine($"Произошла ошибка: {ex.Message}");
                        retryCount++;
                    }
                }
                throw new Exception("Превышено максимальное количество попыток");
            }

            public async Task<T> CallWithRetryAsync<T>(Func<BoomService, Task<T>> func)
            {
                int retryCount = 0;
                while (retryCount < 5)
                {
                    try
                    {
                        return await func(_boomService);
                    }
                    catch (Exception ex)
                    {
                        // Выполните здесь определенное действие
                        // Например, запись в лог
                        await AuthBoomAsync();
                        Console.WriteLine($"Произошла ошибка: {ex.Message}");
                        retryCount++;
                    }
                }
                throw new Exception("Превышено максимальное количество попыток");
            }


        }


        protected static async Task AuthBoomAsync()
        {
            try
            {
                var boomVkToken = await vkService.GetBoomToken();

                var boomToken = await boomService.AuthByTokenAsync(boomVkToken.Token, boomVkToken.Uuid);

                boomService.SetToken(boomToken.AccessToken);


            }
            catch (Exception ex)
            {




            }


        }

        public VK()
        {


        }

        public VK(Login login)
        {
            this.login = login;

        }






        private LoginWay UnwrapTwoFactorWay(LoginWay way)
        {
            if (way == LoginWay.TwoFactorCallReset)
                return LoginWay.CallReset;
            if (way == LoginWay.TwoFactorSms)
                return LoginWay.Sms;
            if (way == LoginWay.TwoFactorPush)
                return LoginWay.Push;
            if (way == LoginWay.TwoFactorEmail)
                return LoginWay.Email;
            return way;
        }


        public async Task<AuthCodeResponse> LoadQrCode(DispatcherQueue dispatcherQueue, bool forceRegenerate = false)
        {
            var result = await _authCategory.GetAuthCodeAsync("VK M Player", forceRegenerate);
            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData qrCodeData = generator.CreateQrCode(new PayloadGenerator.Url(result.AuthUrl), QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    qrCodeImage.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    // Преобразование MemoryStream в IRandomAccessStream
                    InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                    using (DataWriter writer = new DataWriter(randomAccessStream.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(memory.ToArray());
                        await writer.StoreAsync();
                    }

                    var tcs = new TaskCompletionSource<object>();

                    dispatcherQueue.TryEnqueue(async () =>
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(randomAccessStream);

                        result.image = bitmapImage;
                        tcs.SetResult(null);
                    });

                    await tcs.Task;
                    return result;
                }
            }
        }

        private EcosystemProfile Profile;
        string Sid;
        string llogin;

        bool Vk2FaCanUsePassword;
        bool HasAnotherVerificationMethods;

        bool hasOnePass = true;

        private AndroidGrantType _grantType = AndroidGrantType.Password;

        public async Task LoginAsync(string? arg)
        {
            try
            {
                if (string.IsNullOrEmpty(arg))
                    return;


                if (AccountsDB.activeAccount.Token == null)
                {
                    await _vkApi.AuthorizeAsync(new AndroidApiAuthParams());
                }



                var (_, isPhone, authFlow, flowNames, sid, nextStep) = await _authCategory.ValidateAccountAsync(arg, passkeySupported: true, loginWays:
                new[]
                {
                    LoginWay.Push, LoginWay.Sms, LoginWay.CallReset, LoginWay.ReserveCode,
                    LoginWay.Codegen, LoginWay.Email, LoginWay.Passkey
                });


                PInvoke.WebAuthNIsUserVerifyingPlatformAuthenticatorAvailable(out var authenticatorAvailable);
                uint? authenticatorVersion = authenticatorAvailable ? PInvoke.WebAuthNGetApiVersionNumber() : null;


                if (authenticatorVersion <= 4) hasOnePass = false;

                if (authenticatorVersion >= 4 && flowNames.All(b => b != AuthType.Password && b != AuthType.Otp))
                {
                    login.DialogMessageShow("К сожалению, ваша система не поддерживает вход с аккаунтом без пароля, установите пароль или обновите систему.");
                    return;
                }

                Sid = sid;
                llogin = arg;
                Vk2FaCanUsePassword = flowNames.Any(b => b == AuthType.Password);

                

                if (nextStep is null || nextStep.VerificationMethod == LoginWay.Password)
                {

                    if (nextStep == null)
                    {
                        login.Frame.GoBack();
                        return;
                    }


                    //   OpenPage(AccountsWindowPage.EnterPassword);

                    Profile = new EcosystemProfile("Незнакомец", string.Empty, llogin, false, false, "https://vk.com/images/camera_200.png");

                    Password passview = new Password();



                    passview.vk = this;
                    passview.FirstName = Profile.FirstName ?? "Незнакомец";
                    passview.Photo200 = Profile?.Photo200 ?? null;
                    passview.Phone = llogin ?? null;


                    login.Frame.Navigate(typeof(Password), passview, new DrillInNavigationTransitionInfo());
                    return;
                }


                ObservableRangeCollection<EcosystemVerificationMethod> VerificationMethods = new();
                var response = await _ecosystemCategory.GetVerificationMethodsAsync(Sid);
                VerificationMethods.ReplaceRange(response.Methods.Where(b => !string.IsNullOrEmpty(b.Name?.ToString())).OrderBy(b => b.Priority));


                HasAnotherVerificationMethods = nextStep?.HasAnotherVerificationMethods ?? false;

                if (flowNames.Any(b => b == AuthType.Passkey))
                {
                    if (flowNames.Count > 1 && nextStep?.HasAnotherVerificationMethods is null or true)
                    {



                        VerificationMethods.ReplaceRange(response.Methods.Where(b => !string.IsNullOrEmpty(b.Name?.ToString())).OrderBy(b => b.Priority));
                        if (!hasOnePass)
                        {
                            VerificationMethods.Remove(VerificationMethods.FirstOrDefault(x => x.Name == LoginWay.Passkey));
                        }
                        ChooseVerMethods chooseVerMethods = new ChooseVerMethods();

                        chooseVerMethods.VerificationMethods = VerificationMethods;

                        chooseVerMethods.vk = this;

                        login.Frame.Navigate(typeof(ChooseVerMethods), chooseVerMethods, new DrillInNavigationTransitionInfo());

                        return;
                    }
                    await NextStepAsync(LoginWay.Passkey);
                    return;
                }

                await NextStepAsync(nextStep.VerificationMethod, VerificationMethods.Where(ver => ver.Name.Equals(nextStep.VerificationMethod)).FirstOrDefault().Info);
            }
            catch (Exception ex)
            {
                login.Frame.GoBack();
                throw (ex);
             
            }
        }

        public async Task ShowAnotherVerificationMethodsAsync()
        {
            if (Sid is null)
                return;
            login.Frame.Navigate(typeof(waitPage), null, new DrillInNavigationTransitionInfo());

            //   var modal = StaticService.Container.GetRequiredService<LoginVerificationMethodsModalViewModel>();
            ObservableRangeCollection<EcosystemVerificationMethod> VerificationMethods = new();

            var response = await _ecosystemCategory.GetVerificationMethodsAsync(Sid);

            VerificationMethods.ReplaceRange(response.Methods.Where(b => !string.IsNullOrEmpty(b.Name?.ToString())).OrderBy(b => b.Priority));

            if (!hasOnePass)
            {
                VerificationMethods.Remove(VerificationMethods.FirstOrDefault(x => x.Name == LoginWay.Passkey));
            }

            ChooseVerMethods chooseVerMethods = new ChooseVerMethods();

            chooseVerMethods.VerificationMethods = VerificationMethods;



            chooseVerMethods.vk = this;

            login.Frame.Navigate(typeof(ChooseVerMethods), chooseVerMethods, new DrillInNavigationTransitionInfo());
        }





        private async Task AuthPasskeyAsync()
        {
            var hResult = PInvoke.WebAuthNGetCancellationId(out var cancellationId);

            if (hResult.Succeeded)
            {
                hResult = PInvoke.WebAuthNCancelCurrentOperation(cancellationId);
                if (!hResult.Succeeded)
                {
                    login.DialogMessageShow("Не удалось отменить текущий процесс входа, VK M не смог отменить текущий процесс входа! Закройте все диалоги входа с ключем и повторите попытку.");
                    //   _snackbarService.Show();
                    return;
                }
            }

            var (_, passkeyData) = await _authCategory.BeginPasskeyAsync(Sid);

            if (!TryBeginPasskey(passkeyData, out var authenticatorData, out var signature, out var userHandle,
                    out var clientDataJson, out var usedCredential))
            {
                ShowAnotherVerificationMethodsAsync();
                return;
            }
            var json = new JsonObject
            {
                ["response"] = new JsonObject
                {
                    ["authenticatorData"] = authenticatorData.Base64UrlEncode(),
                    ["signature"] = signature.Base64UrlEncode(),
                    ["userHandle"] = userHandle.Base64UrlEncode(),
                    ["clientDataJson"] = clientDataJson.Base64UrlEncode()
                },
                ["id"] = usedCredential.Base64UrlEncode(),
                ["rawId"] = usedCredential.Base64UrlEncode(),
                ["type"] = PInvoke.WebauthnCredentialTypePublicKey,
                ["clientExtensionResults"] = new JsonArray()
            };

            await _vkApiAuth.AuthorizeAsync(new AndroidApiAuthParams(null, Sid, ActionRequestedAsync,
              new[] { LoginWay.Passkey }, PasskeyData: json.ToJsonString())
            {
                AndroidGrantType = _grantType
            });

            await LoggedInAsync();
        }

        private AuthValidatePhoneResponse? Vk2FaResponse { get; set; }
        public async Task NextStepAsync(LoginWay loginWay, string? phone = null)
        {
            login.Frame.Navigate(typeof(waitPage), null, new DrillInNavigationTransitionInfo());
            Vk2FaCanUsePassword = false;

            if (loginWay == LoginWay.Passkey)
            {
                _grantType = AndroidGrantType.Passkey;
                //  OpenPage(AccountsWindowPage.Passkey);
                await AuthPasskeyAsync();
                return;
            }
            _grantType = AndroidGrantType.PhoneConfirmationSid;
            var codeLength = 6;

            if (loginWay == LoginWay.Sms)
            {
                var (_, otpSid, smsInfo, requestedCodeLength) = await _ecosystemCategory.SendOtpSmsAsync(Sid);

                Sid = otpSid;
                codeLength = requestedCodeLength;
                phone ??= smsInfo;
            }
            else if (loginWay == LoginWay.CallReset)
            {
                var (_, otpSid, smsInfo, requestedCodeLength) = await _ecosystemCategory.SendOtpCallResetAsync(Sid);

                Sid = otpSid;
                codeLength = requestedCodeLength;
                phone ??= smsInfo;
            }
            else if (loginWay == LoginWay.Push)
            {
                var (_, otpSid, smsInfo, requestedCodeLength) = await _ecosystemCategory.SendOtpPushAsync(Sid);

                Sid = otpSid;
                codeLength = requestedCodeLength;
                phone ??= smsInfo;
            }
            else if (loginWay.ToString() == "password")
            {

                Password passview = new Password();



                passview.vk = this;
                passview.FirstName = "Незнакомец";
                passview.Photo200 = "null";
                passview.Phone = llogin ?? null;


                login.Frame.Navigate(typeof(Password), passview, new DrillInNavigationTransitionInfo());

                return;
            }

            phone ??= llogin;

            Vk2FaResponse = new(loginWay, LoginWay.None, Sid, 0, codeLength, false, phone);


            OtpCode otpCodee = new OtpCode();

            otpCodee.Info = phone;

            otpCodee.loginWay = loginWay;

            otpCodee.HasAnotherVerificationMethods = HasAnotherVerificationMethods;

            otpCodee.CodeLength = codeLength;

            otpCodee.vk = this;

            login.Frame.Navigate(typeof(OtpCode), otpCodee, new DrillInNavigationTransitionInfo());
            // OpenPage(AccountsWindowPage.Vk2Fa);
        }

        private ValueTask<string> ActionRequestedAsync(LoginWay requestedLoginWay, AuthState state)
        {
            if (state is TwoFactorAuthState twoFactorAuthState)
            {
                Vk2FaResponse = new(twoFactorAuthState.IsSms ? LoginWay.Sms : LoginWay.CallReset, LoginWay.None, null, 0,
                    (int)twoFactorAuthState.CodeLength, true, twoFactorAuthState.PhoneMask);
            }
            else
            {
                if (requestedLoginWay == LoginWay.TwoFactorCallReset)
                    requestedLoginWay = LoginWay.CallReset;
                else if (requestedLoginWay == LoginWay.TwoFactorSms)
                    requestedLoginWay = LoginWay.Sms;
                else if (requestedLoginWay == LoginWay.TwoFactorPush)
                    requestedLoginWay = LoginWay.Push;
                else if (requestedLoginWay == LoginWay.TwoFactorEmail)
                    requestedLoginWay = LoginWay.Email;
                //  else _logger.Error("Unknown login way {LoginWay}", requestedLoginWay);

                Vk2FaResponse = new(requestedLoginWay, LoginWay.None, null, 0, 6, false, llogin);
            }

            _codeTask = new();

            Vk2FaCanUsePassword = false;
            //   OpenPage(AccountsWindowPage.Vk2Fa);

            return new(_codeTask.Task);
        }

        public async Task AuthAsync(string? password)
        {
            login.Frame.Navigate(typeof(waitPage), null, new DrillInNavigationTransitionInfo());
            try
            {


                await _vkApiAuth.AuthorizeAsync(new AndroidApiAuthParams(llogin, Sid, ActionRequestedAsync,
                     new[] { LoginWay.Push, LoginWay.Email }, password)
                {
                    AndroidGrantType = _grantType
                });



                await LoggedInAsync();
            }
            catch (Exception ex)
            {
                login.Frame.GoBack();
            }
        }

        private async Task LoggedInAsync()
        {

            try
            {
                var (token, profile) = await _authCategory.GetExchangeToken();

                AccountsDB.activeAccount.ExchangeToken = token;

                profile = (await api.Users.GetAsync(new List<long> { }, ProfileFields.PhotoMax)).FirstOrDefault();


                AccountsDB.activeAccount.id = profile.Id;
                AccountsDB.activeAccount.Name = $"{profile.FirstName} {profile.LastName}";


                AccountsDB.activeAccount.UserPhoto = (profile.PhotoMax ?? profile.Photo400Orig ?? profile.Photo200Orig ?? profile.Photo200 ?? profile.Photo100 ?? new Uri("https://vk.com/images/camera_200.png")).ToString();

                AccountsDB.activeAccount.Update();


                login.Frame.Navigate(typeof(MainView), null, new DrillInNavigationTransitionInfo());

            }
            catch (Exception ex)
            {
                login.Frame.GoBack();
            }
        }

        public async Task Vk2FaCompleteAsync(string? arg)
        {
            if (string.IsNullOrEmpty(arg) || !int.TryParse(arg, out _) || arg.Length < Vk2FaResponse?.CodeLength)
                return;
            login.Frame.Navigate(typeof(waitPage), null, new DrillInNavigationTransitionInfo());
            if (_grantType == AndroidGrantType.PhoneConfirmationSid)
            {
                EcosystemCheckOtpResponse response = null;
                try
                {
                    response = await _ecosystemCategory.CheckOtpAsync(Sid, Vk2FaResponse!.ValidationType, arg);

                }
                catch (Exception ex)
                {
                    login.Frame.GoBack();
                    return;
                }

                Profile = response.Profile;

                Sid = response.Sid;

                if (response is { ProfileExist: true })
                {
                    if (response.CanSkipPassword)
                    {
                        _grantType = AndroidGrantType.WithoutPassword;
                        await AuthAsync(null);
                    }
                    else
                    {

                        Password passview = new Password();



                        passview.vk = this;
                        passview.FirstName = Profile.FirstName ?? "Незнакомец";
                        passview.Photo200 = Profile?.Photo200 ?? null;
                        passview.Phone = llogin ?? null;


                        login.Frame.Navigate(typeof(Password), passview, new DrillInNavigationTransitionInfo());
                    }

                    return;
                }
                else
                {

                }
            }

            if (_codeTask is not null)
            {
                _codeTask.SetResult(arg);
                _codeTask = null;
                return;
            }

            //      _snackbarService.Show("Ошибка", $"Отправка кода с типом {_grantType} не реализована!");
        }

        private unsafe bool TryBeginPasskey(string passkeyData, out byte[] authenticatorData, out byte[] signature,
            out byte[] userHandle, out string clientDataJson, out byte[] usedCredential)
        {
            var data = JsonSerializer.Deserialize<PasskeyDataResponse>(passkeyData,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            var hWnd = MainWindow.hvn;

            var dwVersion = PInvoke.WebAuthNGetApiVersionNumber();

            var publicKeyPtr = Marshal.StringToHGlobalUni(PInvoke.WebauthnCredentialTypePublicKey);

            var credList = data!.AllowCredentials
                .Where(b => b.Type == PInvoke.WebauthnCredentialTypePublicKey)
                .Select(b =>
                {
                    var id = b.Id.Base64UrlDecode();
                    var ptr = Marshal.AllocHGlobal(id.Length);
                    Marshal.Copy(id, 0, ptr, id.Length);

                    return new Windows.Win32.Networking.WindowsWebServices.WEBAUTHN_CREDENTIAL
                    {
                        dwVersion = dwVersion,
                        pwszCredentialType = (char*)publicKeyPtr,
                        pbId = (byte*)ptr,
                        cbId = (uint)id.Length
                    };
                }).ToArray();





            clientDataJson =
                JsonSerializer.Serialize(new PInvoke.SecurityKeyClientData(PInvoke.SecurityKeyClientData.GetAssertion,
                    data.Challenge, "https://id.vk.ru"));


            byte[] bytes = Encoding.UTF8.GetBytes(clientDataJson);
            IntPtr clientDataJsonPtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, clientDataJsonPtr, bytes.Length);


            var sha256Ptr = Marshal.StringToHGlobalUni("SHA-256");

            HRESULT hResult;
            WEBAUTHN_ASSERTION* assertion;
            try
            {
                fixed (WEBAUTHN_CREDENTIAL* credListPtr = &credList[0])
                    hResult = PInvoke.WebAuthNAuthenticatorGetAssertion(new(hWnd), data.RpId, new()
                    {
                        pbClientDataJSON = (byte*)clientDataJsonPtr,
                        cbClientDataJSON = (uint)Encoding.UTF8.GetByteCount(clientDataJson),
                        dwVersion = 2,
                        pwszHashAlgId = (char*)sha256Ptr,
                    }, new WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS
                    {
                        dwVersion = 4,
                        dwTimeoutMilliseconds = (uint)data.Timeout,
                        CredentialList = new()
                        {
                            cCredentials = (uint)credList.Length,
                            pCredentials = credListPtr
                        },
                        dwUserVerificationRequirement = data.UserVerification == "required" ? 1u : 0,
                        dwAuthenticatorAttachment = PInvoke.WebauthnAuthenticatorAttachmentCrossPlatformU2FV2
                    }, out assertion);
            }
            finally
            {
                Marshal.FreeHGlobal(clientDataJsonPtr);

                foreach (var credential in credList)
                {
                    Marshal.FreeHGlobal((IntPtr)credential.pbId);
                }

                Marshal.FreeHGlobal(sha256Ptr);
                Marshal.FreeHGlobal(publicKeyPtr);
            }

            if (hResult.Failed)
            {
                var ptr = PInvoke.WebAuthNGetErrorName(hResult);
                throw new COMException(new(ptr.AsSpan()), hResult.Value);
            }

            authenticatorData = new ReadOnlySpan<byte>(assertion->pbAuthenticatorData, (int)assertion->cbAuthenticatorData)
                .ToArray();
            signature = new ReadOnlySpan<byte>(assertion->pbSignature, (int)assertion->cbSignature)
                .ToArray();
            userHandle = new ReadOnlySpan<byte>(assertion->pbUserId, (int)assertion->cbUserId)
                .ToArray();
            usedCredential = new ReadOnlySpan<byte>(assertion->Credential.pbId, (int)assertion->Credential.cbId)
                .ToArray();

            PInvoke.WebAuthNFreeAssertion(assertion);

            return hResult.Succeeded;
        }

        public static async Task BoomUpdateToken()
        {
            try
            {
                var boomVkToken = await vkService.GetBoomToken();

                var boomToken = await boomService.AuthByTokenAsync(boomVkToken.Token, boomVkToken.Uuid);

                DB.AccountsDB.activeAccount.BoomToken = boomToken.AccessToken;
                DB.AccountsDB.activeAccount.Update();

                boomService.SetToken(boomToken.AccessToken);
            }
            catch (Exception ex)
            {


            }
        }
       

        /*

        public async Task<string> GetAlbumCover(long? ownerId, long albumId)
        {

            // Убедитесь, что api не является null
            var services = new ServiceCollection();
            services.AddVkNetWithAuth();
            //    services.AddAudioBypass();

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

        */
        public IVkApi getVKAPI()
        {

            return api;
        }

        public List<VkNet.Model.Attachments.Audio> GetUserMusic()
        {

            // Убедитесь, что результат Audio.Search не является null
            VkCollection<VkNet.Model.Attachments.Audio> audio;
            try
            {
                var audios =
                    (api.Audio.GetAsync(new AudioGetParams
                    {
                        OwnerId = AccountsDB.GetActiveAccount().id
                    }).Result);
                return audios.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public static async Task<bool> AddDislike(long Id, long OwnerId)
        {
            try
            {
                var parameters = new VkParameters
                {
                    { "audio_ids", $"{OwnerId}_{Id}" }
                };

                var response = await api.CallAsync("audio.addDislike", parameters);

                if (response.RawJson == "1")
                {
                    return true;
                }
                else
                {


                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> RemoveDislike(long Id, long OwnerId)
        {
            try
            {
                var parameters = new VkParameters
                {
                  { "audio_ids", $"{OwnerId}_{Id}" }
                };

                var response = await api.CallAsync("audio.removeDislike", parameters);

                if (response.RawJson == "1")
                {
                    return true;
                }
                else
                {
                    // Обработка ошибок

                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> deleteFromPlaylist(long Id, long OwnerId, long playlistID)
        {
            try
            {
                var parameters = new VkParameters
                {
                    { "owner_id", OwnerId },
                    { "audio_ids", $"{OwnerId}_{Id}"  },
                    { "playlist_id", playlistID
                    }
                };


                var response = await api.CallAsync("audio.removeFromPlaylist", parameters);

                if (response.RawJson == "1")
                {
                    return true;
                }
                else
                {
                    // Обработка ошибок

                    return false;
                }
            }
            catch { return false; }
        }


        public static async Task sendStartEvent(long Id, long OwnerId, long? playlistID = null)
        {
            try
            {
                var parameters = new VkParameters
                {
                    { "uuid", Guid.NewGuid() },
                    { "audio_id", $"{OwnerId}_{Id}"  },

                };
                if (playlistID != null)
                    parameters.Add("playlist_id", playlistID);

                var response = await api.CallAsync("audio.sendStartEvent", parameters);


            }
            catch (Exception e)
            {

            }
        }


        internal static async Task<string> getUploadServerAsync()
        {

            var parameters = new VkParameters
            {


            };
            try
            {

                var response = await api.CallAsync("audio.getUploadServer", parameters);
                var js = JsonObject.Parse(response.RawJson);
                return js["upload_url"].ToString();
                Console.WriteLine(response.ToString());
            }
            catch
            (Exception e)
            {

            }

            return null;

        }


        internal static async Task deleteAllMusicFromProfile()
        {

           
            try
            {
                var offset = 0;
             
                VkCollection<VkNet.Model.Attachments.Audio> audio;
                List<VkNet.Model.Attachments.Audio> audios = new();
                do
                {
                    AudioGetParams param = new AudioGetParams()
                    {
                        OwnerId = AccountsDB.activeAccount.id,
                        Offset = offset,
                        Count = 6000

                    };
                    offset += 6000;
                    audio = await api.Audio.GetAsync(param);

                    audios.AddRange(audio);
                    
                    

                } while (audio.Count > 0);


                foreach (var item in audios)
                {
                    await api.Audio.DeleteAsync((long)item.Id, (long)item.OwnerId);
                }
            }
            catch
            (Exception e)
            {

            }


        }
    }




}

