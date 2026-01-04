using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    class SnowflakesCountSetting : Slider
    {
        public SnowflakesCountSetting()
        {
            this.Minimum = 10;
            this.Maximum = 500;
            this.StepFrequency = 10;

            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;
            
            int val = 100;
            var set = SettingsTable.GetSetting("snowflakesCount");
            if (set == null)
            {
                SettingsTable.SetSetting("snowflakesCount", "100");
            }
            else
            {
                val = int.Parse(set.settingValue);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;
            
            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Количество снежинок");
            AutomationProperties.SetHelpText(this, "Регулирует количество снежинок в анимации");
        }
       
        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            string a = e.NewValue.ToString();
            Task.Run(() =>
            {
                SettingsTable.SetSetting("snowflakesCount", a);
            });
            MainWindow.mainWindow.Snow.FlakeCount = (int) e.NewValue;
        }
    }
}