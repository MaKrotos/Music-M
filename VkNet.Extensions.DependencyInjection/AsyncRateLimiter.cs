namespace VkNet.Extensions.DependencyInjection;

public class AsyncRateLimiter : IAsyncRateLimiter
{
    private readonly SemaphoreSlim _semaphore;
    private readonly CancellationTokenSource _tokenSource = new();
    
    public AsyncRateLimiter(TimeSpan window, int maxRequestsPerWindow)
    {
        Window = window;
        MaxRequestsPerWindow = maxRequestsPerWindow;
        _semaphore = new(maxRequestsPerWindow);
    }

    public TimeSpan Window { get; }
    public int MaxRequestsPerWindow { get; }
    
    public async ValueTask WaitNextAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (TryGetNext())
                return;

            var source = cancellationToken != default
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _tokenSource.Token)
                : null;

            await _semaphore.WaitAsync(source?.Token ?? cancellationToken);

            source?.Dispose();
        }
        finally
        {
            await ReleaseAsync();
        }
    }

    private async Task ReleaseAsync()
    {
        await Task.Delay(Window, _tokenSource.Token).ConfigureAwait(false);
        _semaphore.Release();
    }

    public ValueTask WaitNextAsync(int timeout) => WaitNextAsync(TimeSpan.FromMilliseconds(timeout));

    public async ValueTask WaitNextAsync(TimeSpan timeout)
    {
        using var source = new CancellationTokenSource(timeout);
        await WaitNextAsync(source.Token);
    }

    public bool TryGetNext()
    {
        if (_semaphore.CurrentCount == 0) return false;
        
        _semaphore.Wait();
        return true;
    }

    public void Dispose()
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
        _semaphore.Dispose();
    }
}