namespace VkNet.Extensions.DependencyInjection;

public interface IVkTokenStore
{
    string Token { get; }
    Task SetAsync(string? token, DateTimeOffset? expiration = null);
}