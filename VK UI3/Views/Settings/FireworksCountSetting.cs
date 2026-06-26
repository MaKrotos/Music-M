using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    class FireworksCountSetting : Slider
    {
        public FireworksCountSetting()
        {
            this.Minimum = 1;
            this.Maximum = 20;
            this.StepFrequency = 1;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            int val = 5;
            var set = SettingsTable.GetSetting("fireworksRocketCount");
            if (set == null)
            {
                SettingsTable.SetSetting("fireworksRocketCount", "5");
            }
            else
            {
                val = int.Parse(set.settingValue);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Количество ракет");
            AutomationProperties.SetHelpText(this, "Регулирует количество одновременно летящих ракет фейерверка");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = e.NewValue.ToString();
            Task.Run(() =>
            {
                SettingsTable.SetSetting("fireworksRocketCount", a);
            });
            MainWindow.mainWindow.Fireworks.RocketCount = (int)e.NewValue;
        }
    }
}