using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;

namespace VK_UI3.Helpers
{
    public class SnowflakeCanvas : Canvas
    {
        private Random _random = new Random();
        private DispatcherTimer _timer = new DispatcherTimer();

        public SnowflakeCanvas()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            // Создаем новую снежинку
            Ellipse snowflake = new Ellipse
            {
                Width = _random.Next(2, 5),
                Height = _random.Next(2, 5),
                Fill = new SolidColorBrush(Microsoft.UI.Colors.White)
            };

            // Устанавливаем начальное положение снежинки
            SetLeft(snowflake, _random.NextDouble() * ActualWidth);
            SetTop(snowflake, 0);

            // Добавляем снежинку на холст
            Children.Add(snowflake);

            // Удаляем снежинки, которые вышли за пределы холста
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                Ellipse oldSnowflake = Children[i] as Ellipse;
                if (GetTop(oldSnowflake) > ActualHeight)
                {
                    Children.RemoveAt(i);
                }
                else
                {
                    // Перемещаем снежинку вниз
                    SetTop(oldSnowflake, GetTop(oldSnowflake) + oldSnowflake.Height);
                }
            }
        }
    }


}
