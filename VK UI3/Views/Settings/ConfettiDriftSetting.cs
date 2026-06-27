using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Globalization;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiDriftSetting : Slider
    {
        public ConfettiDriftSetting()
        {
            this.Minimum = -2;
            this.Maximum = 2;
            this.StepFrequency = 0.1;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            double val = 0;
            var set = SettingsTable.GetSetting("confettiDrift");
            if (set == null)
            {
                SettingsTable.SetSetting("confettiDrift", "0");
            }
            else
            {
                val = double.Parse(set.settingValue, CultureInfo.InvariantCulture);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            AutomationProperties.SetName(this, "Дрифт конфетти");
            AutomationProperties.SetHelpText(this, "Устанавливает горизонтальный дрифт частиц конфетти от -2 до 2");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = e.NewValue.ToString("F1", CultureInfo.InvariantCulture);
            Task.Run(() =>
            {
                SettingsTable.SetSetting("confettiDrift", a);
            });
        }
    }
}