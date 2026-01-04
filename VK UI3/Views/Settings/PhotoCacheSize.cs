using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    class PhotoCacheSize : Slider
    {
        public PhotoCacheSize()
        {
         

            this.Minimum = 50;
            this.Maximum = 10000;
            this.StepFrequency = 10;


            this.Style = Application.Current.Resources["DefaultSliderStyle"] as Style;
            
            double val = 100;
            var set = SettingsTable.GetSetting("photoCacheSize");
            if (set == null)
            {
                SettingsTable.SetSetting("photoCacheSize", "100");
            }
            else
            {
                val = double.Parse(set.settingValue);
            }

            this.Value = val;

            this.ValueChanged += OnValueChanged;
            
            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Размер кеша фотографий");
            AutomationProperties.SetHelpText(this, "Устанавливает размер кеша для хранения фотографий в мегабайтах");
        }
       

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

            string a = e.NewValue.ToString();
            Task.Run(() =>
            {
                SettingsTable.SetSetting("photoCacheSize", a);
            });
        }
    }
}
