using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Globalization;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiScalarSetting : Slider
    {
        public ConfettiScalarSetting()
        {
            this.Minimum = 0.1;
            this.Maximum = 3;
            this.StepFrequency = 0.1;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            double val = 1;
            var set = SettingsTable.GetSetting("confettiScalar");
            if (set == null)
            {
                SettingsTable.SetSetting("confettiScalar", "1");
            }
            else
            {
                val = double.Parse(set.settingValue, CultureInfo.InvariantCulture);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            AutomationProperties.SetName(this, "Масштаб частиц конфетти");
            AutomationProperties.SetHelpText(this, "Устанавливает масштаб частиц конфетти от 0.1 до 3");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = e.NewValue.ToString("F1", CultureInfo.InvariantCulture);
            Task.Run(() =>
            {
                SettingsTable.SetSetting("confettiScalar", a);
            });
        }
    }
}