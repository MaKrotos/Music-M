using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.Enums.Filters;

namespace VkNet.AudioBypassService.Abstractions.Categories;

public interface IAuthCategory
{
    Task<AuthValidateAccountResponse> ValidateAccountAsync(string login, bool forcePassword = false, bool passkeySupported = false, [CanBeNull] IEnumerable<LoginWay> loginWays = null);
    Task<AuthValidatePhoneResponse> ValidatePhoneAsync(string phone, string sid, bool allowCallReset = true, [CanBeNull] IEnumerable<LoginWay> loginWays = null);

    Task<AuthCodeResponse> GetAuthCodeAsync(string deviceName, bool forceRegenerate = true);
    
    Task<AuthCheckResponse> CheckAuthCodeAsync(string authHash, string token);

    Task<string> connect_code_auth(string token, string uuid);

    [ItemCanBeNull] Task<TokenInfo> RefreshTokensAsync(string oldToken, string exchangeToken);

    Task<ExchangeTokenResponse> GetExchangeToken([CanBeNull] UsersFields fields = null);
    
    Task<PasskeyBeginResponse> BeginPasskeyAsync(string sid);
    
    /// <summary>
    /// Обновление токенов через OAuth endpoint, используемый в мобильном приложении VK
    /// </summary>
    /// <param name="refreshToken">Токен для обновления</param>
    /// <param name="deviceId">Идентификатор устройства</param>
    /// <returns>Новые токены доступа</returns>
    Task<VkConnectResponse> RefreshTokenAsync(string refreshToken, string deviceId);
}