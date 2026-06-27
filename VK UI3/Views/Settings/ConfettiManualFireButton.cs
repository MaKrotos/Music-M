using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiManualFireButton : Button
    {
        public ConfettiManualFireButton()
        {
            this.Content = "Запустить сейчас";
            this.Click += OnClick;
            this.HorizontalAlignment = HorizontalAlignment.Left;

            this.Style = Application.Current.Resources["DefaultButtonStyle"] as Style;

            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Запустить конфетти сейчас");
            AutomationProperties.SetHelpText(this, "Запускает анимацию конфетти с выбранным пресетом");
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow?.ConfettiService?.FireConfetti();
        }
    }
}