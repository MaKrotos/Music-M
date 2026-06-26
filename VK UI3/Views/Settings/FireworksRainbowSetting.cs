using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class FireworksRainbowSetting : CheckBox
    {
        public FireworksRainbowSetting()
        {
            this.Content = "Разноцветные фейерверки";

            this.Checked += FireworksRainbowSetting_Checked;
            this.Unchecked += FireworksRainbowSetting_Unchecked;
            this.Loaded += FireworksRainbowSetting_Loaded;

            // Получение стиля из ресурсов
            Style style = Application.Current.Resources["DefaultCheckBoxStyle"] as Style;

            // Установка стиля
            this.Style = style;

            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Разноцветные фейерверки");
            AutomationProperties.SetHelpText(this, "Включает или выключает разноцветный режим фейерверков. При включении каждый фейерверк будет случайного цвета.");
        }

        private void FireworksRainbowSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var setting = SettingsTable.GetSetting("fireworksUseRainbowColors");
                this.IsChecked = setting != null && setting.settingValue.Equals("1");
            });
        }

        private void FireworksRainbowSetting_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("fireworksUseRainbowColors", "0");
            if (MainWindow.mainWindow?.Fireworks != null)
                MainWindow.mainWindow.Fireworks.UseRainbowColors = false;
        }

        private void FireworksRainbowSetting_Checked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("fireworksUseRainbowColors", "1");
            if (MainWindow.mainWindow?.Fireworks != null)
                MainWindow.mainWindow.Fireworks.UseRainbowColors = true;
        }
    }
}