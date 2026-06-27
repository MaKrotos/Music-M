using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiStartVelocitySetting : Slider
    {
        public ConfettiStartVelocitySetting()
        {
            this.Minimum = 1;
            this.Maximum = 100;
            this.StepFrequency = 1;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            double val = 45;
            var set = SettingsTable.GetSetting("confettiStartVelocity");
            if (set == null)
            {
                SettingsTable.SetSetting("confettiStartVelocity", "45");
            }
            else
            {
                val = double.Parse(set.settingValue);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            AutomationProperties.SetName(this, "Скорость конфетти");
            AutomationProperties.SetHelpText(this, "Устанавливает начальную скорость частиц конфетти от 1 до 100");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = ((int)e.NewValue).ToString();
            Task.Run(() =>
            {
                SettingsTable.SetSetting("confettiStartVelocity", a);
            });
        }
    }
}