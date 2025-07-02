using System.Reflection;
using Microsoft.Extensions.Logging;
using VkNet.Abstractions.Core;
using VkNet.Exception;
using VkNet.Utils;

namespace VkNet.Extensions.DependencyInjection;

/// <inheritdoc />
public class AsyncCaptchaHandler : ICaptchaHandler
{
    private static readonly MethodInfo PerformAsyncMethod = typeof(AsyncCaptchaHandler).GetMethod(nameof(PerformAsync), BindingFlags.Instance | BindingFlags.NonPublic)!;
    
    private readonly IAsyncCaptchaSolver? _captchaSolver;
    private readonly ILogger<CaptchaHandler>? _logger;

    /// <inheritdoc cref="T:CaptchaHandler" />
    public AsyncCaptchaHandler(ILogger<CaptchaHandler>? logger, IAsyncCaptchaSolver? captchaSolver)
    {
        _logger = logger;
        _captchaSolver = captchaSolver;
    }

    /// <inheritdoc />
    public int MaxCaptchaRecognitionCount { get; set; } = 3;

    /// <inheritdoc />
    public T? Perform<T>(Func<ulong?, string?, T> action)
    {
        if (typeof(T).IsAssignableTo(typeof(Task)))
        {
            return (T?)PerformAsyncMethod.MakeGenericMethod(typeof(T).GenericTypeArguments[0], typeof(T))
                .Invoke(this, new object[] {action});
        }
        
        var context = new CaptchaSolveContext(MaxCaptchaRecognitionCount);
        var flag = false;
        var obj = default (T);
        do
        {
            try
            {
                obj = action(context.CaptchaSidTemp, context.CaptchaKeyTemp);
                flag = true;
            }
            catch (CaptchaNeededException ex)
            {
                if (!RepeatSolveCaptchaAsync(ex, context).Result)
                    break;
            }
        }
        while (context.RemainingSolveAttempts > 0 && !flag);
        if (flag || !context.CaptchaSidTemp.HasValue)
            return obj;

        throw new CaptchaNeededException(new()
        {
            CaptchaSid = context.CaptchaSidTemp.Value
        });
    }

    private async Task<T?> PerformAsync<T, T2>(Func<ulong?, string?, T2> action) where T2 : Task<T>
    {
        var context = new CaptchaSolveContext(MaxCaptchaRecognitionCount);
        var flag = false;
        var obj = default (T);
        do
        {
            try
            {
                obj = await action(context.CaptchaSidTemp, context.CaptchaKeyTemp);
                flag = true;
            }
            catch (CaptchaNeededException ex)
            {
                if (!await RepeatSolveCaptchaAsync(ex, context))
                    break;
            }
        }
        while (context.RemainingSolveAttempts > 0 && !flag);
        if (flag || !context.CaptchaSidTemp.HasValue)
            return obj;

        throw new CaptchaNeededException(new()
        {
            CaptchaSid = context.CaptchaSidTemp.Value
        });
    }
    
    private async Task<bool> RepeatSolveCaptchaAsync(
        CaptchaNeededException captchaNeededException,
        CaptchaSolveContext context)
    {
        _logger?.LogWarning("Повторная обработка капчи");
        if (context.RemainingSolveAttempts < MaxCaptchaRecognitionCount && _captchaSolver is not null)
            await _captchaSolver.SolveFailedAsync();
        if (context.RemainingSolveAttempts <= 0)
            return false;
        context.CaptchaSidTemp = captchaNeededException.Sid;
        var task = _captchaSolver?.SolveAsync(captchaNeededException.Img!.AbsoluteUri, redirectUri: captchaNeededException.RedirectUri.ToString());
        context.CaptchaKeyTemp =  task.HasValue ? await task.Value : null;
        context.RemainingSolveAttempts--;
        return context.CaptchaKeyTemp is not null;
    }

    private class CaptchaSolveContext
    {
        public int RemainingSolveAttempts;
        public ulong? CaptchaSidTemp;
        public string? CaptchaKeyTemp;

        public CaptchaSolveContext(int maxSolveAttempts)
        {
            RemainingSolveAttempts = maxSolveAttempts;
        }
    }
}