using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiRandomColorSetting : CheckBox
    {
        public ConfettiRandomColorSetting()
        {
            this.Content = "Случайные цвета";

            this.Checked += ConfettiRandomColorSetting_Checked;
            this.Unchecked += ConfettiRandomColorSetting_Unchecked;
            this.Loaded += ConfettiRandomColorSetting_Loaded;

            // Получение стиля из ресурсов
            Style style = Application.Current.Resources["DefaultCheckBoxStyle"] as Style;

            // Установка стиля
            this.Style = style;

            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Случайные цвета конфетти");
            AutomationProperties.SetHelpText(this, "Включает или выключает случайные цвета для конфетти. При включении цвета будут выбираться случайным образом.");
        }

        private void ConfettiRandomColorSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var setting = SettingsTable.GetSetting("confettiUseRandomColors");
                this.IsChecked = setting != null && setting.settingValue.Equals("1");
            });
        }

        private void ConfettiRandomColorSetting_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("confettiUseRandomColors", "0");
        }

        private void ConfettiRandomColorSetting_Checked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("confettiUseRandomColors", "1");
        }
    }
}