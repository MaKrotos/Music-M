using System;
using System.Threading.Tasks;
using VK_UI3.DB;
using VkNet.AudioBypassService.Abstractions;
using VkNet.Extensions.DependencyInjection;
using static VK_UI3.DB.AccountsDB;

public class RegistryTokenStore : IVkTokenStore, IExchangeTokenStore, IDeviceIdStore
{
    private DateTimeOffset? Expiration
    {
        get => activeAccount?.Expiration;
        set
        {
            if (activeAccount != null)
            {
                activeAccount.Expiration = value ?? DateTimeOffset.MaxValue;
                DatabaseHandler.getConnect().Update(activeAccount);
            }
        }
    }

    public string Token
    {
        get
        {
            return activeAccount.Token ?? activeAccount.AnonToken ??
                 throw new InvalidOperationException("Authorization is required");

        }
    }

    public Task SetAsync(string? token, DateTimeOffset? expiration = null)
    {

        if (token?.StartsWith("anonym") == true)
        {
            activeAccount.AnonToken = token;
        }
        else
        {
            activeAccount.Token = token;
        }

        Expiration = expiration;

        return Task.CompletedTask;
    }

    public ValueTask<string?> GetExchangeTokenAsync() => new ValueTask<string?>(activeAccount.ExchangeToken);

    public ValueTask SetExchangeTokenAsync(string token)
    {

        activeAccount.ExchangeToken = token;

        return ValueTask.CompletedTask;
    }

    public ValueTask<string?> GetDeviceIdAsync() => new ValueTask<string?>(activeAccount.DeviceId);

    public ValueTask SetDeviceIdAsync(string deviceId)
    {

        activeAccount.DeviceId = deviceId;

        return ValueTask.CompletedTask;
    }


}
