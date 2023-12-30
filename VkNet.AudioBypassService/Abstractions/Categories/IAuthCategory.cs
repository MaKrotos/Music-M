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
    
    Task<AuthCheckResponse> CheckAuthCodeAsync(string authHash);
    
    [ItemCanBeNull] Task<TokenInfo> RefreshTokensAsync(string oldToken, string exchangeToken);

    Task<ExchangeTokenResponse> GetExchangeToken([CanBeNull] UsersFields fields = null);
    
    Task<PasskeyBeginResponse> BeginPasskeyAsync(string sid);
}