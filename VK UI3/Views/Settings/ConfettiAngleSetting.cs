using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiAngleSetting : Slider
    {
        public ConfettiAngleSetting()
        {
            this.Minimum = 0;
            this.Maximum = 360;
            this.StepFrequency = 1;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            double val = 90;
            var set = SettingsTable.GetSetting("confettiAngle");
            if (set == null)
            {
                SettingsTable.SetSetting("confettiAngle", "90");
            }
            else
            {
                val = double.Parse(set.settingValue);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            AutomationProperties.SetName(this, "Угол вылета конфетти");
            AutomationProperties.SetHelpText(this, "Устанавливает угол вылета конфетти от 0 до 360 градусов");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = ((int)e.NewValue).ToString();
            Task.Run(() =>
            {
                SettingsTable.SetSetting("confettiAngle", a);
            });
        }
    }
}