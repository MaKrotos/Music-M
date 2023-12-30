namespace VkNet.Extensions.DependencyInjection;

public interface IAsyncRateLimiter : IDisposable
{
    public TimeSpan Window { get; }
    public int MaxRequestsPerWindow { get; }
    ValueTask WaitNextAsync(CancellationToken cancellationToken = default);
    ValueTask WaitNextAsync(int timeout);
    ValueTask WaitNextAsync(TimeSpan timeout);
    bool TryGetNext();
}