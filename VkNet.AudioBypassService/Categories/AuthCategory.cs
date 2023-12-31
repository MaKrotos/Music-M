using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using VkNet.Abstractions;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.Enums.Filters;
using VkNet.Extensions.DependencyInjection;
using VkNet.Utils;
using IAuthCategory = VkNet.AudioBypassService.Abstractions.Categories.IAuthCategory;

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
        const string url =
            "https://id.vk.com/qr_auth?scheme=vkcom_dark&app_id=7913379&origin=https%3A%2F%2Fvk.com&initial_stats_info=eyJzb3VyY2UiOiJtYWluIiwic2NyZWVuIjoic3RhcnQifQ%3D%3D";

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

    public async Task<AuthCodeResponse> GetAuthCodeAsync(string deviceName, bool forceRegenerate = true)
    {
        return await _apiInvoke.CallAsync<AuthCodeResponse>("auth.getAuthCode", new()
        {
            { "device_name", deviceName },
            { "force_regenerate", forceRegenerate },
            { "auth_code_flow", false },
            { "client_id", 7913379 },
            { "anonymous_token", _anonToken ?? await GetAnonTokenAsync() },
            { "verification_hash", _authVerifyHash }
        }, true);
    }

    public async Task<AuthCheckResponse> CheckAuthCodeAsync(string authHash)
    {
        return await _apiInvoke.CallAsync<AuthCheckResponse>("auth.checkAuthCode", new()
        {
            { "auth_hash", authHash },
            { "client_id", 7913379 },
            { "anonymous_token", _anonToken ?? await GetAnonTokenAsync() }
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
        var response = await _restClient.PostAsync(new("https://api.vk.com/oauth/passkey_begin"), new VkParameters
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
        
        return JsonSerializer.Deserialize<PasskeyBeginResponse>(response.Value, options);
    }
}