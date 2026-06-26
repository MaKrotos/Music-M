using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class FireworksSetting : CheckBox
    {
        public FireworksSetting()
        {
            this.Content = "Включить фейерверки";

            this.Checked += FireworksSetting_Checked;
            this.Unchecked += FireworksSetting_Unchecked;
            this.Loaded += FireworksSetting_Loaded;

            // Получение стиля из ресурсов
            Style style = Application.Current.Resources["DefaultCheckBoxStyle"] as Style;

            // Установка стиля
            this.Style = style;

            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Включить фейерверки");
            AutomationProperties.SetHelpText(this, "Включает или выключает анимацию фейерверков в приложении");
        }

        private void FireworksSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var setting = SettingsTable.GetSetting("fireworksEnabled");
                this.IsChecked = setting != null && setting.settingValue.Equals("1");
            });
        }

        private void FireworksSetting_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("fireworksEnabled", "0");
            MainWindow.mainWindow.Fireworks.Stop();
        }

        private void FireworksSetting_Checked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("fireworksEnabled", "1");
            MainWindow.mainWindow.Fireworks.Start();
        }
    }
}