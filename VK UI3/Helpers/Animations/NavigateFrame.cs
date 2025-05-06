using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using VK_UI3.Views;
using static VK_UI3.Views.SectionView;

namespace KrotosNavigationFrame
{
    public class FramesKrotos
    {
        public Storyboard Storyboard = new Storyboard();
        public Frame Frame { get; set; }

       public  SectionType sectionType;
    }

    internal class NavigateFrame : Grid
    {
        private List<FramesKrotos> _BackStack = new List<FramesKrotos>();

        public bool CanGoBack
        {
            get
            {
                return (_BackStack.Count > 0);
            }
        }

        public void ClearBackStack()
        {
            foreach (var item in _BackStack)
            {
                this.Children.Remove(item.Frame);
            }
            _BackStack.Clear();
        }
        public FramesKrotos frameCurrent{ get { return currentFrame; } }

        private FramesKrotos currentFrame;

        public event NavigatedEventHandler Navigated;

        public void Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride)
        {
            this.DispatcherQueue.TryEnqueue(async () => {
                if (currentFrame != null)
                {
                    _BackStack.Add(currentFrame);
                    var curr = currentFrame;
                    // Не скрываем сразу, а анимируем
                    AnimateOpacity(curr.Frame, 1, 0, 300, () => {
                        curr.Frame.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    });
                }

                Frame frame = new Frame();
                frame.Opacity = 0;
                frame.ContentTransitions = [new NavigationThemeTransition()];

                // Добавляем новый фрейм поверх старых
                this.Children.Add(frame);

                currentFrame = new FramesKrotos() { Frame = frame   };
                if (parameter is WaitParameters waitParameters)
                {
                    currentFrame.sectionType = waitParameters.sectionType;
                }

                // Показываем новый фрейм с анимацией
                frame.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                AnimateOpacity(frame, 0, 1, 300, null);

                currentFrame.Frame.Navigate(sourcePageType, parameter, infoOverride);
                Navigated?.Invoke(this, null);
            });
        }

        public void GoBack()
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                if (!this.CanGoBack)
                    return;

                if (currentFrame != null)
                {
                    var curr = currentFrame;
                    // Анимируем исчезновение текущего фрейма
                    AnimateOpacity(curr.Frame, 1, 0, 300, () =>
                    {
                        this.Children.Remove(curr.Frame);
                        _BackStack.Remove(curr);

                    });

                    // После анимации показываем предыдущий фрейм
                    currentFrame = _BackStack.Last();
                    _BackStack.Remove(currentFrame);

                    currentFrame.Frame.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    AnimateOpacity(currentFrame.Frame, 0, 1, 300, null);

                    Navigated?.Invoke(this, null);
                }
            });
        }

        private void AnimateOpacity(UIElement target, double from, double to, double durationMs, Action onCompleted)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EnableDependentAnimation = true
            };

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, "Opacity");

            if (onCompleted != null)
            {
                storyboard.Completed += (s, e) => onCompleted();
            }

            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }
}