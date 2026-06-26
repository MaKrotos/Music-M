using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Globalization;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    class FireworksSpeedSetting : Slider
    {
        public FireworksSpeedSetting()
        {
            this.Minimum = 0.1;
            this.Maximum = 3.0;
            this.StepFrequency = 0.1;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;

            double val = 1.0;
            var set = SettingsTable.GetSetting("fireworksAnimationSpeed");
            if (set == null)
            {
                SettingsTable.SetSetting("fireworksAnimationSpeed", "1.0");
            }
            else
            {
                val = double.Parse(set.settingValue, CultureInfo.InvariantCulture);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;

            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Скорость анимации фейерверков");
            AutomationProperties.SetHelpText(this, "Регулирует скорость анимации фейерверков");
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = e.NewValue.ToString("0.0", CultureInfo.InvariantCulture);
            Task.Run(() =>
            {
                SettingsTable.SetSetting("fireworksAnimationSpeed", a);
            });
            MainWindow.mainWindow.Fireworks.AnimationSpeed = e.NewValue;
        }
    }
}