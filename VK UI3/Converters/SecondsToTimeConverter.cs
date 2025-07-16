using Microsoft.UI.Xaml.Data;
using System;

namespace VK_UI3.Converters
{
    internal class SecondsToTimeConverter : IValueConverter
    {
        public SecondsToTimeConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double totalMilliseconds = 0;
            if (value is long l)
                totalMilliseconds = l;
            else if (value is int i)
                totalMilliseconds = i;
            else if (value is double d)
                totalMilliseconds = d;
            else
                return "00:00";

            var time = TimeSpan.FromMilliseconds(totalMilliseconds);
            if (time.TotalHours >= 1)
                return time.ToString(@"hh\:mm\:ss");
            else
                return time.ToString(@"mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string timeString)
            {
                string[] parts = timeString.Split(':');
                int hours = 0, minutes = 0, seconds = 0;
                if (parts.Length == 3)
                {
                    hours = int.Parse(parts[0]);
                    minutes = int.Parse(parts[1]);
                    seconds = int.Parse(parts[2]);
                }
                else if (parts.Length == 2)
                {
                    minutes = int.Parse(parts[0]);
                    seconds = int.Parse(parts[1]);
                }
                else
                {
                    throw new ArgumentException("Invalid time format");
                }
                var ts = new TimeSpan(hours, minutes, seconds);
                return (long)ts.TotalMilliseconds;
            }
            throw new ArgumentException("Invalid time format");
        }
    }
}
