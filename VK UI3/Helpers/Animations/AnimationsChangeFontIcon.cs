using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeFontIcon
    {
        string fontIconNow = null;
        Storyboard storyboard = new Storyboard();
        FontIcon iconControl = null;
        DispatcherQueue dispatcherQueue = null;

        public AnimationsChangeFontIcon(FontIcon iconControl, DispatcherQueue dispatcher)
        {
            fontIconNow = iconControl.Glyph;
            this.iconControl = iconControl;
            this.dispatcherQueue = dispatcher;
        }
        public async void ChangeFontIconWithAnimation(string newFontIcon)
        {
            dispatcherQueue.TryEnqueue(async () =>
            {
                if (fontIconNow != null && fontIconNow == newFontIcon)
                    return;

                fontIconNow = newFontIcon;
                if (storyboard.GetCurrentState() == ClockState.Active)
                {
                    storyboard.Pause();
                }

                var animation = new DoubleAnimation
                {
                    From = iconControl.Opacity,
                    To = 0.0,
                    Duration = TimeSpan.FromMilliseconds(50),
                };

                Storyboard.SetTarget(animation, iconControl);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Stop();
                storyboard.Children.Clear();
                storyboard.Children.Add(animation);

                EventHandler<object> handler = null;
                handler = (s, e) =>
                {
                    // Отписываемся от события после его выполнения
                    storyboard.Completed -= handler;

                    iconControl.Glyph = newFontIcon;

                    var animation = new DoubleAnimation
                    {
                        From = iconControl.Opacity,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(50),
                    };
                    Storyboard.SetTarget(animation, iconControl);
                    Storyboard.SetTargetProperty(animation, "Opacity");
                    storyboard.Stop();
                    storyboard.Children.Clear();
                    storyboard.Children.Add(animation);
                    storyboard.Begin();


                };

                // Отписываемся от предыдущих обработчиков перед подпиской на новый
                storyboard.Completed -= handler;
                storyboard.Completed += handler;

                storyboard.Begin();
            });
        }
    }
}
