using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class StatSlyToggleSetting : CheckBox
    {
        public StatSlyToggleSetting()
        {
            try
            {
                this.Content = "Отправлять анонимную статистику использования";

                this.Checked += StatSlyToggleSetting_Checked;
                this.Unchecked += StatSlyToggleSetting_Unchecked;
                this.Loaded += StatSlyToggleSetting_Loaded;

                // Получение стиля из ресурсов
                Style style = Application.Current.Resources["DefaultCheckBoxStyle"] as Style;

                // Установка стиля
                this.Style = style;

                // Добавляем свойства доступности для экранного диктера
                AutomationProperties.SetName(this, "Отправлять анонимную статистику использования");
                AutomationProperties.SetHelpText(this, "Отправляет анонимную статистику использования и отчёты об ошибках на сервер StatSly для улучшения приложения. Вы можете отключить сбор в любое время.");
            }
            catch { }
        }

        private void StatSlyToggleSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var setting = DB.SettingsTable.GetSetting("StatSlyEnabled");

                if (setting == null)
                {
                    // По умолчанию статистика включена (opt-out)
                    this.IsChecked = true;
                    DB.SettingsTable.SetSetting("StatSlyEnabled", "1");
                    return;
                }
                this.IsChecked = setting.settingValue.Equals("1") ? true : false;
            });
        }

        private void StatSlyToggleSetting_Unchecked(object sender, RoutedEventArgs e)
        {
            DB.SettingsTable.SetSetting("StatSlyEnabled", "0");
        }

        private void StatSlyToggleSetting_Checked(object sender, RoutedEventArgs e)
        {
            DB.SettingsTable.SetSetting("StatSlyEnabled", "1");
        }
    }
}