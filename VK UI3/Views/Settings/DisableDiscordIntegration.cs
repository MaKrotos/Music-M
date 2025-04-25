using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Settings
{
    public sealed class DisableDiscordIntegration : CheckBox
    {
        public DisableDiscordIntegration()
        {
            this.Content = "Отключить интеграцию с Discord";

            this.Checked += StartUpSetting_Checked;
            this.Unchecked += StartUpSetting_Unchecked;
            this.Loaded += StartUpSetting_Loaded;

            // Получение стиля из ресурсов
            Style style = Application.Current.Resources["DefaultCheckBoxStyle"] as Style;
            
            // Установка стиля
            this.Style = style;
        }

        private void StartUpSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
               var setting = DB.SettingsTable.GetSetting("DisableDiscordIntegration");

                if (setting == null)
                    return;
                this.IsChecked = setting.settingValue.Equals("1") ? true : false;
            });
        }

        private void StartUpSetting_Unchecked(object sender, RoutedEventArgs e)
        {
            DB.SettingsTable.SetSetting("DisableDiscordIntegration", "0");
        }

        private void StartUpSetting_Checked(object sender, RoutedEventArgs e)
        {
            DB.SettingsTable.SetSetting("DisableDiscordIntegration", "1");
        }
    }
}
