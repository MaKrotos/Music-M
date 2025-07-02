using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Views.ModalsPages;
using VkNet.Extensions.DependencyInjection;
using VkNet.Model.Attachments;
using System.Collections.Generic;
using Microsoft.UI.Dispatching;

namespace VK_UI3.VKs.Ext;

public class CaptchaSolverService : IAsyncCaptchaSolver
{

    private readonly IServiceProvider _serviceProvider;

    List<ContentDialog> contentDialogs = new List<ContentDialog>();

    public CaptchaSolverService(
        IServiceProvider serviceProvider)
    {

        _serviceProvider = serviceProvider;
    }

    public async ValueTask<string?> SolveAsync(string url, string? redirectUri = null)
    {
        foreach (var item in contentDialogs)
        {
            if (item != null)
                item.Hide();
        }
        contentDialogs.Clear();

        TaskCompletionSource<string?> Submitted = new();
        CaptchaEnter captchaEnter = new CaptchaEnter();
        captchaEnter.Submitted = Submitted;
        captchaEnter.CaptchaUri = url;
        captchaEnter.RedirectUri = redirectUri;

        TaskCompletionSource<string?> dialogResult = new();

        MainWindow.dispatcherQueue.TryEnqueue(async () =>
        {
            ContentDialog dialog = new CustomDialog();
            dialog.Transitions = new TransitionCollection
                            {
                                new PopupThemeTransition()
                            };
            dialog.XamlRoot = MainWindow.contentFrame.XamlRoot;

            var a = new CaptchaEnter(captchaEnter);
            a.ParentDialog = dialog;
            dialog.Content = a;
            dialog.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            contentDialogs.Add(dialog);

            await dialog.ShowAsync();

            var enterede = await Submitted.Task;

            dialog.Hide();
            contentDialogs.Remove(dialog);

            dialogResult.SetResult(enterede);
        });

        var result = await dialogResult.Task;
        return result;
    }

    public ValueTask SolveFailedAsync()
    {
        // _snackbarService.Show("Ошибка!", "Вы ввели неправильную капчу");
        return ValueTask.CompletedTask;
    }
}
