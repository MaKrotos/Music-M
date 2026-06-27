using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiSpreadSetting : Slider
    {
        public ConfettiSpreadSetting()
        {
            this.Minimum = 0;
            this.Maximum = 360;
            this.StepFrequency = 1;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            double val = 45;
            var set = SettingsTable.GetSetting("confettiSpread");
            if (set == null)
            {
                SettingsTable.SetSetting("confettiSpread", "45");
            }
            else
            {
                val = double.Parse(set.settingValue);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            AutomationProperties.SetName(this, "Разброс конфетти");
            AutomationProperties.SetHelpText(this, "Устанавливает разброс конфетти от 0 до 360 градусов");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = ((int)e.NewValue).ToString();
            Task.Run(() =>
            {
                SettingsTable.SetSetting("confettiSpread", a);
            });
        }
    }
}