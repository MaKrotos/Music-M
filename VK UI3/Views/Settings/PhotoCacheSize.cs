using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vkPosterBot.DB;

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
