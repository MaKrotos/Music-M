using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;
using VK_UI3.DB;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiRandomOriginSetting : CheckBox
    {
        public ConfettiRandomOriginSetting()
        {
            this.Content = "Случайное место вылета";

            this.Checked += ConfettiRandomOriginSetting_Checked;
            this.Unchecked += ConfettiRandomOriginSetting_Unchecked;
            this.Loaded += ConfettiRandomOriginSetting_Loaded;

            Style style = Application.Current.Resources["DefaultCheckBoxStyle"] as Style;
            this.Style = style;

            AutomationProperties.SetName(this, "Случайное место вылета конфетти");
            AutomationProperties.SetHelpText(this, "Включает или выключает случайное место появления конфетти. При включении конфетти будут вылетать из случайных точек на экране.");
        }

        private void ConfettiRandomOriginSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var setting = SettingsTable.GetSetting("confettiUseRandomOrigin");
                this.IsChecked = setting == null || setting.settingValue.Equals("1");
            });
        }

        private void ConfettiRandomOriginSetting_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("confettiUseRandomOrigin", "0");
        }

        private void ConfettiRandomOriginSetting_Checked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("confettiUseRandomOrigin", "1");
        }
    }
}