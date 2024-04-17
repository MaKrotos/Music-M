using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeIcon
    {
        Symbol? symbolNow = null;
        Storyboard storyboard = new Storyboard();
        SymbolIcon iconControl = null;

        public AnimationsChangeIcon(SymbolIcon iconControl)
        {
            symbolNow = iconControl.Symbol;
            this.iconControl = iconControl;
        }

        public async void ChangeSymbolIconWithAnimation(Symbol newSymbol)
        {
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

            // Устанавливаем цель анимации
            Storyboard.SetTarget(animation, iconControl);
            Storyboard.SetTargetProperty(animation, "Opacity");

            // Очищаем Storyboard и добавляем новую анимацию
            storyboard.Stop();
            storyboard.Children.Clear();
            storyboard.Children.Add(animation);

            // Обрабатываем событие завершения анимации
            EventHandler<object> handler = null;
            handler = (s, e) =>
            {
                // Отписываемся от события
                storyboard.Completed -= handler;
                // Меняем иконку после завершения анимации
                iconControl.Symbol = newSymbol;

                var animation = new DoubleAnimation
                {
                    From = iconControl.Opacity,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(50),
                };
                // Устанавливаем цель анимации
                Storyboard.SetTarget(animation, iconControl);
                Storyboard.SetTargetProperty(animation, "Opacity");
                // Очищаем Storyboard и добавляем новую анимацию
                storyboard.Stop();
                storyboard.Children.Clear();
                storyboard.Children.Add(animation);
                storyboard.Begin();

        
            };
            storyboard.Completed -= handler;
            storyboard.Completed += handler;
            storyboard.Begin();
        }
    }
}
