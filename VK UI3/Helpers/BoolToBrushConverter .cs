using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Helpers
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Transparent);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Transparent);
        }
    }
}
