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

    public async ValueTask<string?> SolveAsync(string url)
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
        captchaEnter.captchaUri = url;

        //.contentFrame.Navigate(typeof(CaptchaEnter), captchaEnter);


        ContentDialog dialog = new CustomDialog();

        dialog.Transitions = new TransitionCollection
                        {
                            new PopupThemeTransition()
                        };

        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = MainWindow.contentFrame.XamlRoot;


        var a = new CaptchaEnter(captchaEnter);
        dialog.Content = a;
        dialog.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        dialog.ShowAsync();

        var enterede =  await Submitted.Task;


        dialog.Hide();
        contentDialogs.Remove(dialog);


        contentDialogs.Add(dialog);


        return new (enterede);
    }

    public ValueTask SolveFailedAsync()
    {
        // _snackbarService.Show("Ошибка!", "Вы ввели неправильную капчу");
        return ValueTask.CompletedTask;
    }
}
