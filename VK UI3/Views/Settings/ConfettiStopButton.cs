using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;
using VK_UI3.DB;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiStopButton : Button
    {
        public ConfettiStopButton()
        {
            this.Content = "Выключить";
            this.Click += OnClick;
            this.HorizontalAlignment = HorizontalAlignment.Left;

            this.Style = Application.Current.Resources["DefaultButtonStyle"] as Style;

            AutomationProperties.SetName(this, "Выключить конфетти");
            AutomationProperties.SetHelpText(this, "Немедленно останавливает текущую анимацию конфетти");
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow?.ConfettiService?.StopContinuousMode();
        }
    }
}