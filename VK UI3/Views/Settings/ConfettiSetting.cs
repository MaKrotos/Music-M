using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiSetting : CheckBox
    {
        private bool _isLoaded = false;

        public ConfettiSetting()
        {
            this.Content = "Включить конфетти";

            this.Checked += ConfettiSetting_Checked;
            this.Unchecked += ConfettiSetting_Unchecked;
            this.Loaded += ConfettiSetting_Loaded;

            // Получение стиля из ресурсов
            Style style = Application.Current.Resources["DefaultCheckBoxStyle"] as Style;

            // Установка стиля
            this.Style = style;

            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Включить конфетти");
            AutomationProperties.SetHelpText(this, "Включает или выключает непрерывную анимацию конфетти в приложении");
        }

        private void ConfettiSetting_Loaded(object sender, RoutedEventArgs e)
        {
            var setting = SettingsTable.GetSetting("confettiEnabled");
            this.IsChecked = setting != null && setting.settingValue.Equals("1");
            _isLoaded = true;
        }

        private void ConfettiSetting_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("confettiEnabled", "0");
            if (_isLoaded)
            {
                MainWindow.mainWindow?.ConfettiService?.StopContinuousMode();
            }
        }

        private void ConfettiSetting_Checked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("confettiEnabled", "1");
            if (_isLoaded)
            {
                MainWindow.mainWindow?.ConfettiService?.StartContinuousMode();
            }
        }
    }
}