using System.Threading.Tasks;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Utils;
using VkNet.Extensions.DependencyInjection;
using VkNet.Model;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Flows;

internal class WithoutPasswordAuthorizationFlow : VkAndroidAuthorizationBase
{
    public WithoutPasswordAuthorizationFlow(IVkTokenStore tokenStore, FakeSafetyNetClient safetyNetClient,
        IDeviceIdStore deviceIdStore, IVkApiVersionManager versionManager, ILanguageService languageService,
        IAsyncRateLimiter rateLimiter, IRestClient restClient, ICaptchaHandler captchaHandler,
        LibVerifyClient libVerifyClient) : base(tokenStore, safetyNetClient, deviceIdStore, versionManager,
        languageService, rateLimiter, restClient, captchaHandler, libVerifyClient)
    {
    }

    protected override Task<AuthorizationResult> AuthorizeAsync(AndroidApiAuthParams authParams)
    {
        return AuthAsync(authParams);
    }

    protected override async ValueTask<VkParameters> BuildParameters(AndroidApiAuthParams authParams)
    {
        var parameters = await base.BuildParameters(authParams);
        
        parameters.Add("username", authParams.Login);
        parameters.Add("flow_type", "auth_without_password");
        parameters.Add("2fa_supported", true);
        parameters.Add("vk_connect_auth", true);
        
        return parameters;
    }
    
    /// <summary>
    /// Обновление токенов через OAuth endpoint, используемый в мобильном приложении VK
    /// </summary>
    /// <param name="refreshToken">Токен для обновления</param>
    /// <param name="deviceId">Идентификатор устройства</param>
    /// <returns>Новые токены доступа</returns>
    public async Task<VkConnectResponse> RefreshTokenAsync(string refreshToken, string deviceId)
    {
        return await base.RefreshTokenAsync(refreshToken, deviceId);
    }
}