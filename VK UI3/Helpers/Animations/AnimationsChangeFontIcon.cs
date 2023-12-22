using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeFontIcon
    {
        string fontIconNow = null;
        Storyboard storyboard = null;
        FontIcon iconControl = null;

        public AnimationsChangeFontIcon(FontIcon iconControl)
        {
            fontIconNow = iconControl.Glyph;
            this.iconControl = iconControl;
        }

        public async void ChangeFontIconWithAnimation(string newFontIcon)
        {
            if (fontIconNow != null && fontIconNow == newFontIcon)
                return;

            fontIconNow = newFontIcon;
            if (storyboard == null) storyboard = new Storyboard();
            if (storyboard.GetCurrentState() == ClockState.Active)
            {
                storyboard.Pause();

            }

            // Создаем анимацию прозрачности
            var animation = new DoubleAnimation
            {
                From = iconControl.Opacity,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(50),
            };

            // Создаем объект Storyboard для управления анимацией
            storyboard = new Storyboard();
            Storyboard.SetTarget(animation, iconControl);
            Storyboard.SetTargetProperty(animation, "Opacity");

            // Добавляем анимацию в Storyboard
            storyboard.Children.Add(animation);

            // Обрабатываем событие завершения анимации
            storyboard.Completed += (s, e) =>
            {
                // Меняем иконку после завершения анимации
                iconControl.Glyph = newFontIcon;

                var animation = new DoubleAnimation
                {
                    From = iconControl.Opacity,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(50),

                };
                // Создаем объект Storyboard для управления анимацией
                storyboard = new Storyboard();
                Storyboard.SetTarget(animation, iconControl);
                Storyboard.SetTargetProperty(animation, "Opacity");
                // Добавляем анимацию в Storyboard
                storyboard.Children.Add(animation);
                storyboard.Begin();
            };

            // Запускаем анимацию
            storyboard.Begin();

        }
    }
}
