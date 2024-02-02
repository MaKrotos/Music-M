using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VK_UI3.Views.ModalsPages;
using VkNet.Extensions.DependencyInjection;


namespace VK_UI3.VKs.Ext;

public class CaptchaSolverService : IAsyncCaptchaSolver
{
   
    private readonly IServiceProvider _serviceProvider;

    public CaptchaSolverService(
        IServiceProvider serviceProvider)
    {
    
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<string?> SolveAsync(string url)
    {
        TaskCompletionSource<string?> Submitted = new();
        CaptchaEnter captchaEnter = new CaptchaEnter();
        captchaEnter.Submitted = Submitted;
        captchaEnter.captchaUri = url;

        MainWindow.contentFrame.Navigate(typeof(CaptchaEnter), captchaEnter);

        return new (await Submitted.Task);
    }

    public ValueTask SolveFailedAsync()
    {
     // _snackbarService.Show("РћС€РёР±РєР°!", "Р’С‹ РІРІРµР»Рё РЅРµРїСЂР°РІРёР»СЊРЅСѓСЋ РєР°РїС‡Сѓ");
        return ValueTask.CompletedTask;
    }
}

