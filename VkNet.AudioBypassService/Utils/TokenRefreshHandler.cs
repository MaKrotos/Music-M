using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.Extensions.DependencyInjection;
using IAuthCategory = VkNet.AudioBypassService.Abstractions.Categories.IAuthCategory;

namespace VkNet.AudioBypassService.Utils;

public class TokenRefreshHandler : ITokenRefreshHandler
{
    private readonly IExchangeTokenStore _exchangeTokenStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly IVkTokenStore _tokenStore;

    public TokenRefreshHandler(IExchangeTokenStore exchangeTokenStore, IServiceProvider serviceProvider, IVkTokenStore tokenStore)
    {
        _exchangeTokenStore = exchangeTokenStore;
        _serviceProvider = serviceProvider;
        _tokenStore = tokenStore;
    }

    public async Task<string> RefreshTokenAsync(string oldToken)
    {
        if (oldToken.StartsWith("anonym"))
        {
            var auth = _serviceProvider.GetRequiredService<IVkApiAuthAsync>();
            
            await auth.AuthorizeAsync(new AndroidApiAuthParams());
            
            return _tokenStore.Token;
        }
        
        if (await _exchangeTokenStore.GetExchangeTokenAsync() is not { } exchangeToken)
            return null;

        var authCategory = _serviceProvider.GetRequiredService<IAuthCategory>();

        var tokenInfo = await authCategory.RefreshTokensAsync(oldToken, exchangeToken);
        
        if (tokenInfo is null)
            return null;

        var (token, expiresIn) = tokenInfo;

        await _tokenStore.SetAsync(token, DateTimeOffset.Now + TimeSpan.FromSeconds(expiresIn));

        var (newExchangeToken, _) = await authCategory.GetExchangeToken();
        
        await _exchangeTokenStore.SetExchangeTokenAsync(newExchangeToken);

        return token;
    }
}