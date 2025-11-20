using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Abstractions;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Utils;
using VkNet.Enums.Filters;
using VkNet.Extensions.DependencyInjection;
using VkNet.Utils;
using IAuthCategory = VkNet.AudioBypassService.Abstractions.Categories.IAuthCategory;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace VkNet.AudioBypassService.Categories;

public partial class AuthCategory : IAuthCategory
{
    private readonly IVkApiInvoke _apiInvoke;
    private readonly IRestClient _restClient;
    private readonly IVkTokenStore _tokenStore;
    private readonly IVkApiVersionManager _versionManager;

    [CanBeNull] private string _anonToken;
    [CanBeNull] private string _authVerifyHash;

    public AuthCategory(IVkApiInvoke apiInvoke, IRestClient restClient, IVkTokenStore tokenStore, IVkApiVersionManager versionManager)
    {
        _apiInvoke = apiInvoke;
        _restClient = restClient;
        _tokenStore = tokenStore;
        _versionManager = versionManager;
    }
    private static partial Regex AuthVerifyHashRegex();

    private static partial Regex AnonTokenRegex();
    private static partial Regex AnonTokenRegex()
    {
        return new Regex("\"anonymous_token\"\\:\\s?\"(?<token>[\\w\\.\\=\\-]*)\"\\,?", RegexOptions.Multiline);
    }

    private static partial Regex AuthVerifyHashRegex()
    {
        return new Regex("\"code_auth_verification_hash\"\\:\\s?\"(?<hash>[\\w\\.\\=\\-]*)\"\\,?", RegexOptions.Multiline);
    }

    private async ValueTask<string> GetAnonTokenAsync()
    {

        try
        {
            var a = await _apiInvoke.CallAsync("auth.getAnonymToken", new()
            {
                  { "client_id", 7913379 },
            });
        }
        catch (System.Exception ex)
        {
        
        }

        const string url =
            "https://id.vk.ru/qr_auth?scheme=vkcom_dark&app_id=7913379&origin=https%3A%2F%2Fvk.ru&initial_stats_info=eyJzb3VyY2UiOiJtYWluIiwic2NyZWVuIjoic3RhcnQifQ%3D%3D";

        var response = await _restClient.GetAsync(new(url), ImmutableDictionary<string, string>.Empty, Encoding.UTF8);

        _authVerifyHash = AuthVerifyHashRegex().Match(response.Value).Groups["hash"].Value;



        return _anonToken = AnonTokenRegex().Match(response.Value).Groups["token"].Value;
    }

    public Task<AuthValidateAccountResponse> ValidateAccountAsync(string login, bool forcePassword = false, bool passkeySupported = false, IEnumerable<LoginWay> loginWays = null)
    {
        return _apiInvoke.CallAsync<AuthValidateAccountResponse>("auth.validateAccount", new()
        {
            { "login", login },
            { "force_password", forcePassword },
            { "supported_ways", loginWays },
            { "flow_type", "auth_without_password" },
            { "api_id", 2274003 },
            { "passkey_supported", passkeySupported }
        });
    }

    public Task<AuthValidatePhoneResponse> ValidatePhoneAsync(string phone, string sid, bool allowCallReset = true, IEnumerable<LoginWay> loginWays = null)
    {
        return _apiInvoke.CallAsync<AuthValidatePhoneResponse>("auth.validatePhone", new()
        {
            { "phone", phone },
            { "sid", sid },
            { "supported_ways", loginWays },
            { "flow_type", "tg_flow" },
            { "allow_callreset", allowCallReset }
        });
    }

    

    public async Task<string> connect_code_auth(string token, string uuid)
    {
        try
        {
    
            using (var client = new HttpClient())
            {
                var parameters = new Dictionary<string, string>
            {
                { "token", token },
                { "uuid", uuid },
                { "version", "1" },
                { "app_id", "7913379" },
                { "flow_start_state", "" },
                { "is_external_carousel", "" },
                { "oauth_version", "" },
                { "sid", "" },
                { "oauth_force_hash", "0" },
                { "is_registration", "0" },
                { "oauth_response_type", "" },
                { "vkid_oauth_hash", "" },
                { "is_oauth_migrated_flow", "0" },
                { "oauth_state", "" },
                { "to", "aHR0cHM6Ly92ay5jb20=" }
            };
           

                var content = new FormUrlEncodedContent(parameters);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                client.DefaultRequestHeaders.Add("Origin", "https://id.vk.ru");
                var response = await client.PostAsync("https://login.vk.ru/?act=connect_code_auth", content);
                response.EnsureSuccessStatusCode();
                var str = await response.Content.ReadAsStringAsync();



                JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);
                var (type, loginResponse) = JsonSerializer.Deserialize<LoginResponse<LoginResponseAccessToken>>(str, _jsonSerializerOptions);
                JObject jo = JObject.Parse(str);
                if (jo != null && jo["data"] != null && jo["data"]["access_token"] != null)
                {

                    var next_url = jo["data"]["next_step_url"].ToString();
                    var nextResponse = await client.GetAsync(next_url);
                    nextResponse.EnsureSuccessStatusCode();

                    return jo["data"]["access_token"].ToString();
                }

                return null;
            }
        }
        catch (System.Exception ex)
        {
            // Логирование ошибки
            return null;
        }
    }

    public async Task<AuthCodeResponse> GetAuthCodeAsync(string deviceName, bool forceRegenerate = true)
    {
        try
        {
            string token = _anonToken ?? await GetAnonTokenAsync();
            using (var client = new HttpClient())
            {
                var parameters = new Dictionary<string, string>
            {
                { "device_name", "Windows NT 10.0; Win64; x64" },
                { "force_regenerate", forceRegenerate.ToString() },
                { "auth_code_flow", "0" },
                { "anonymous_token", _anonToken },
                { "verification_hash", "" },
                { "is_switcher_flow", "" },
                { "access_token", "" }
            };
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                client.DefaultRequestHeaders.Add("Origin", "https://id.vk.ru");

                var content = new FormUrlEncodedContent(parameters);
                var response = await client.PostAsync("https://api.vk.ru/method/auth.getAuthCode?v=5.207&client_id=7913379", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync())["response"];
                var authCodeResponse = JsonSerializer.Deserialize<AuthCodeResponse>(jsonResponse.ToString());
                authCodeResponse.Token = token;

                return authCodeResponse;
            }
        }
        catch (System.Exception ex)
        {
            // Логирование ошибки
            return null;
        }
    }

    public async Task<AuthCheckResponse> CheckAuthCodeAsync(string authHash, string token)
    {
        return  await _apiInvoke.CallAsync<AuthCheckResponse>("auth.checkAuthCode?v=5.207&client_id=7913379", new()
        {
            { "auth_hash", authHash },
            { "web_auth", 1 },
            { "anonymous_token", token }
        }, true);
    }

    public async Task<TokenInfo> RefreshTokensAsync(string oldToken, string exchangeToken)
    {
        var response = await _apiInvoke.CallAsync<AuthRefreshTokensResponse>("auth.refreshTokens", new()
        {
            { "exchange_tokens", exchangeToken },
            { "scope", "all" },
            {"initiator", "expired_token"},
            {"active_index", 0},
            { "api_id", 2274003 },
            { "client_id", 2274003 },
            { "client_secret", "hHbZxrka2uZ6jB1inYsH" },
        }, true);
        
        return response.Success.Count > 0 ? response.Success[0].AccessToken : null;
    }
    
    /// <summary>
    /// Обновление токенов через OAuth endpoint, используемый в мобильном приложении VK
    /// </summary>
    /// <param name="refreshToken">Токен для обновления</param>
    /// <param name="deviceId">Идентификатор устройства</param>
    /// <returns>Новые токены доступа</returns>
    public async Task<VkConnectResponse> RefreshTokenAsync(string refreshToken, string deviceId)
    {
        var parameters = new VkParameters
        {
            { "device_id", deviceId },
            { "device_os", "android" },
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken }
        };

        var response = await _restClient.PostAsync(new Uri("https://api.vk.ru/oauth/token"), parameters, Encoding.UTF8);
        
        var obj = VkErrors.IfErrorThrowException(response.Value ?? response.Message);
        VkAuthErrors.IfErrorThrowException(obj);

        return obj.ToObject<VkConnectResponse>();
    }

    public Task<ExchangeTokenResponse> GetExchangeToken(UsersFields fields = null)
    {
        return _apiInvoke.CallAsync<ExchangeTokenResponse>("execute.getUserInfo", new()
        {
            { "func_v", 30 },
            { "androidVersion", 32 },
            { "androidManufacturer", "VK M" },
            { "androidModel", "VK M" },
            { "needExchangeToken", true },
            { "fields", fields }
        });
    }

    public async Task<PasskeyBeginResponse> BeginPasskeyAsync(string sid)
    {
        var response = await _restClient.PostAsync(new("https://api.vk.ru/oauth/passkey_begin"), new VkParameters
        {
            { "sid", sid },
            { "anonymous_token", _tokenStore.Token },
            { "v", _versionManager.Version },
            { "https", true }
        }, Encoding.UTF8);

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        
        return System.Text.Json.JsonSerializer.Deserialize<PasskeyBeginResponse>(response.Value, options);
    }
}