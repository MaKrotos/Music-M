using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Globalization;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiGravitySetting : Slider
    {
        public ConfettiGravitySetting()
        {
            this.Minimum = 0;
            this.Maximum = 5;
            this.StepFrequency = 0.1;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            double val = 1;
            var set = SettingsTable.GetSetting("confettiGravity");
            if (set == null)
            {
                SettingsTable.SetSetting("confettiGravity", "1");
            }
            else
            {
                val = double.Parse(set.settingValue, CultureInfo.InvariantCulture);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            AutomationProperties.SetName(this, "Гравитация конфетти");
            AutomationProperties.SetHelpText(this, "Устанавливает гравитацию для частиц конфетти от 0 до 5");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = e.NewValue.ToString("F1", CultureInfo.InvariantCulture);
            Task.Run(() =>
            {
                SettingsTable.SetSetting("confettiGravity", a);
            });
        }
    }
}