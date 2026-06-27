using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;
using VK_UI3.DB;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiResetButton : Button
    {
        public ConfettiResetButton()
        {
            this.Content = "Сбросить настройки";
            this.Click += OnClick;
            this.HorizontalAlignment = HorizontalAlignment.Left;

            this.Style = Application.Current.Resources["DefaultButtonStyle"] as Style;

            AutomationProperties.SetName(this, "Сбросить настройки конфетти");
            AutomationProperties.SetHelpText(this, "Сбрасывает все настройки конфетти на значения по умолчанию");
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            // Сбрасываем все настройки конфетти на значения по умолчанию
            SettingsTable.SetSetting("confettiEnabled", "0");
            SettingsTable.SetSetting("confettiPreset", "FireBasic");
            SettingsTable.SetSetting("confettiUseRandomColors", "0");
            SettingsTable.SetSetting("confettiUseRandomOrigin", "1");
            SettingsTable.SetSetting("confettiParticleCount", "100");
            SettingsTable.SetSetting("confettiAngle", "90");
            SettingsTable.SetSetting("confettiSpread", "45");
            SettingsTable.SetSetting("confettiStartVelocity", "45");
            SettingsTable.SetSetting("confettiGravity", "1");
            SettingsTable.SetSetting("confettiDrift", "0");
            SettingsTable.SetSetting("confettiTicks", "200");
            SettingsTable.SetSetting("confettiScalar", "1");
            SettingsTable.SetSetting("confettiShapes", "square,circle");
            SettingsTable.SetSetting("confettiContinuousInterval", "8");
            SettingsTable.SetSetting("confettiColor", "#FFFF69B4");

            // Останавливаем непрерывный режим
            MainWindow.mainWindow?.ConfettiService?.StopContinuousMode();
        }
    }
}