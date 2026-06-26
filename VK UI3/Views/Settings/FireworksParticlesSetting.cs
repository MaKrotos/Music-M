using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    class FireworksParticlesSetting : Slider
    {
        public FireworksParticlesSetting()
        {
            this.Minimum = 500;
            this.Maximum = 5000;
            this.StepFrequency = 100;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            int val = 2000;
            var set = SettingsTable.GetSetting("fireworksMaxParticles");
            if (set == null)
            {
                SettingsTable.SetSetting("fireworksMaxParticles", "2000");
            }
            else
            {
                val = int.Parse(set.settingValue);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Максимум частиц");
            AutomationProperties.SetHelpText(this, "Ограничивает максимальное количество частиц фейерверка для производительности");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = e.NewValue.ToString();
            Task.Run(() =>
            {
                SettingsTable.SetSetting("fireworksMaxParticles", a);
            });
            MainWindow.mainWindow.Fireworks.MaxParticles = (int)e.NewValue;
        }
    }
}