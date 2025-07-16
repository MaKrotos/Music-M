using System;
using Microsoft.UI.Xaml.Data;

namespace VK_UI3.Converters
{
    public class LongToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is long l) return (double)l;
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double d) return (long)d;
            return 0L;
        }
    }
} 