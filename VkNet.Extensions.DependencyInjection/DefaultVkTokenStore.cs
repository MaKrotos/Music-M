namespace VkNet.Extensions.DependencyInjection;

public class DefaultVkTokenStore : IVkTokenStore
{
    private string? _token;
    private DateTimeOffset? _expiration;

    public string Token
    {
        get
        {
            var token = _token ?? throw new InvalidOperationException("Authorization is required");
            if (_expiration.HasValue && _expiration.Value < DateTimeOffset.Now)
                throw new InvalidOperationException("Token has expired");

            return token;
        }
    }

    public Task SetAsync(string? token, DateTimeOffset? expiration = null)
    {
        _token = token;
        _expiration = expiration ?? DateTimeOffset.MaxValue;
        return Task.CompletedTask;
    }
}