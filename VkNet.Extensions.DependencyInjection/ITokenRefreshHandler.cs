namespace VkNet.Extensions.DependencyInjection;

public interface ITokenRefreshHandler
{
    Task<string?> RefreshTokenAsync(string oldToken);
}