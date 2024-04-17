using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Converters
{
    internal class SecondsToTimeConverter : IValueConverter
    {
        public SecondsToTimeConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double seconds)
            {
                TimeSpan time = TimeSpan.FromSeconds(seconds);
                return time.ToString(@"mm\:ss");
            }
            if (value is int secondsS)
            {
                TimeSpan time = TimeSpan.FromSeconds(secondsS);
                return time.ToString(@"mm\:ss");
            }
            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string timeString)
            {
                string[] parts = timeString.Split(':');
                if (parts.Length == 2)
                {
                    try
                    {
                        int minutes = Int32.Parse(parts[0]);
                        int seconds = Int32.Parse(parts[1]);
                        return minutes * 60 + seconds;
                    }
                    catch (FormatException)
                    {
                        // Обработка исключений, связанных с форматом
                    }
                }
            }
            throw new ArgumentException("Invalid time format");
        }

    }
}
