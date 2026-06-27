using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiTicksSetting : Slider
    {
        public ConfettiTicksSetting()
        {
            this.Minimum = 10;
            this.Maximum = 500;
            this.StepFrequency = 1;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            int val = 200;
            var set = SettingsTable.GetSetting("confettiTicks");
            if (set == null)
            {
                SettingsTable.SetSetting("confettiTicks", "200");
            }
            else
            {
                val = int.Parse(set.settingValue);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            AutomationProperties.SetName(this, "Длительность жизни частиц конфетти");
            AutomationProperties.SetHelpText(this, "Устанавливает длительность жизни частиц конфетти в тиках от 10 до 500");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = ((int)e.NewValue).ToString();
            Task.Run(() =>
            {
                SettingsTable.SetSetting("confettiTicks", a);
            });
        }
    }
}