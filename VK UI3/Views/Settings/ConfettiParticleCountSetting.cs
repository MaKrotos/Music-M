using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiParticleCountSetting : Slider
    {
        public ConfettiParticleCountSetting()
        {
            this.Minimum = 1;
            this.Maximum = 500;
            this.StepFrequency = 1;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            int val = 100;
            var set = SettingsTable.GetSetting("confettiParticleCount");
            if (set == null)
            {
                SettingsTable.SetSetting("confettiParticleCount", "100");
            }
            else
            {
                val = int.Parse(set.settingValue);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            AutomationProperties.SetName(this, "Количество частиц конфетти");
            AutomationProperties.SetHelpText(this, "Устанавливает количество частиц конфетти от 1 до 500");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = ((int)e.NewValue).ToString();
            Task.Run(() =>
            {
                SettingsTable.SetSetting("confettiParticleCount", a);
            });
        }
    }
}