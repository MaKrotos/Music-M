using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiContinuousIntervalSetting : Slider
    {
        public ConfettiContinuousIntervalSetting()
        {
            this.Minimum = 1;
            this.Maximum = 60;
            this.StepFrequency = 1;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            int val = 8;
            var set = SettingsTable.GetSetting("confettiContinuousInterval");
            if (set == null)
            {
                SettingsTable.SetSetting("confettiContinuousInterval", "8");
            }
            else
            {
                val = int.Parse(set.settingValue);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            AutomationProperties.SetName(this, "Интервал непрерывного режима конфетти");
            AutomationProperties.SetHelpText(this, "Устанавливает интервал между запусками конфетти в непрерывном режиме от 1 до 60 секунд");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = ((int)e.NewValue).ToString();
            Task.Run(() =>
            {
                SettingsTable.SetSetting("confettiContinuousInterval", a);
            });
        }
    }
}