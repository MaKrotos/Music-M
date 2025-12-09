using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VK_UI3.DB;

namespace VK_UI3.Views.Settings
{
    public sealed class SnowflakesSetting : CheckBox
    {
        public SnowflakesSetting()
        {
            this.Content = "Включить снежинки";

            this.Checked += SnowflakesSetting_Checked;
            this.Unchecked += SnowflakesSetting_Unchecked;
            this.Loaded += SnowflakesSetting_Loaded;

            // Получение стиля из ресурсов
            Style style = Application.Current.Resources["DefaultCheckBoxStyle"] as Style;
            
            // Установка стиля
            this.Style = style;
        }

        private void SnowflakesSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var setting = SettingsTable.GetSetting("snowflakesEnabled");
                this.IsChecked = setting != null && setting.settingValue.Equals("1");
            });
        }

        private void SnowflakesSetting_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("snowflakesEnabled", "0");
            MainWindow.mainWindow.Snow.Stop();
        }

        private void SnowflakesSetting_Checked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("snowflakesEnabled", "1");
            MainWindow.mainWindow.Snow.Start();
        }
    }
}