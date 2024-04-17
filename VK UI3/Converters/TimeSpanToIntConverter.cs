using Microsoft.UI.Xaml.Data;
using System;

namespace VK_UI3.Converters
{
    internal class TimeSpanToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan a = (TimeSpan)value;
            return a.Milliseconds * 0.0001;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double a = (double)value;
            return TimeSpan.FromSeconds(a);


        }
    }
}
