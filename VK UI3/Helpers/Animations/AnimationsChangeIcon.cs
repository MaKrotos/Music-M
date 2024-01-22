using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeIcon
    {
        Symbol? symbolNow = null;
        Storyboard storyboard = null;
        SymbolIcon iconControl = null;

        public AnimationsChangeIcon(SymbolIcon iconControl)
        {
            symbolNow = iconControl.Symbol;
            this.iconControl = iconControl;
        }

        public async void ChangeSymbolIconWithAnimation(Symbol newSymbol)
        {

            if (storyboard == null) storyboard = new Storyboard();

            if (symbolNow != null && (symbolNow == newSymbol && iconControl.Symbol == newSymbol)) return;

            symbolNow = newSymbol;

            if (storyboard.GetCurrentState() == ClockState.Active)
            {
                storyboard.Pause();

            }

            // Создаем анимацию прозрачности
            var animation = new DoubleAnimation
            {
                From = iconControl.Opacity,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(100),
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
                iconControl.Symbol = newSymbol;

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
