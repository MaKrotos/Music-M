using VkNet.Abstractions;
using VkNet.Abstractions.Authorization;
using VkNet.Model;

namespace VkNet.Extensions.DependencyInjection;

public class VkApiAuth : IVkApiAuthAsync
{
    private IApiAuthParams? _lastAuthParams;
    private readonly IAuthorizationFlow _authorizationFlow;
    private readonly IVkTokenStore _tokenStore;
    private readonly ITokenRefreshHandler? _tokenRefreshHandler;

    public VkApiAuth(IAuthorizationFlow authorizationFlow, IVkTokenStore tokenStore, ITokenRefreshHandler? tokenRefreshHandler = null)
    {
        _authorizationFlow = authorizationFlow;
        _tokenStore = tokenStore;
        _tokenRefreshHandler = tokenRefreshHandler;
    }

    public void Authorize(IApiAuthParams @params)
    {
        AuthorizeAsync(@params).GetAwaiter().GetResult();
    }

    public void Authorize(ApiAuthParams @params)
    {
        Authorize((IApiAuthParams)@params);
    }

    public void RefreshToken(Func<string>? code = null)
    {
        RefreshTokenAsync(code).GetAwaiter().GetResult();
    }

    public void LogOut()
    {
        LogOutAsync().GetAwaiter().GetResult();
    }

    public bool IsAuthorized { get; private set; }
    public async Task AuthorizeAsync(IApiAuthParams @params)
    {
        if (!string.IsNullOrEmpty(@params.AccessToken))
        {
            await _tokenStore.SetAsync(@params.AccessToken);
            return;
        }

        _authorizationFlow.SetAuthorizationParams(@params);

        var result = await _authorizationFlow.AuthorizeAsync();

        await _tokenStore.SetAsync(result.AccessToken,
                                   result.ExpiresIn > 0
                                       ? DateTimeOffset.Now + TimeSpan.FromSeconds(result.ExpiresIn)
                                       : null);

        _lastAuthParams = @params;
    }

    public Task RefreshTokenAsync(Func<string>? code = null)
    {
        if (_lastAuthParams is null || !_lastAuthParams.IsValid)
        {
            throw new InvalidOperationException();
        }

        if (code is not null)
            _lastAuthParams.TwoFactorAuthorization = code;

        return _tokenRefreshHandler?.RefreshTokenAsync(_tokenStore.Token) ?? AuthorizeAsync(_lastAuthParams);
    }

    public Task LogOutAsync()
    {
        IsAuthorized = false;
        return _tokenStore.SetAsync(null);
    }
}