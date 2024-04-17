using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace VK_UI3.Helpers.Animations
{
    public class AnimatedButton : Button
    {


        private CancellationTokenSource cts;

        public static readonly DependencyProperty OriginalWidthProperty = DependencyProperty.Register(
          "OriginalWidth", typeof(double), typeof(AnimatedButton), new PropertyMetadata(default(double)));

        public double? OriginalWidth
        {
            get { return (double)GetValue(OriginalWidthProperty); }
            set { SetValue(OriginalWidthProperty, value); }
        }

        public static readonly DependencyProperty OriginalHeightProperty = DependencyProperty.Register(
            "OriginalHeight", typeof(double), typeof(AnimatedButton), new PropertyMetadata(default(double)));

        public double? OriginalHeight
        {
            get { return (double)GetValue(OriginalHeightProperty); }
            set { SetValue(OriginalHeightProperty, value); }
        }

        public static readonly DependencyProperty OriginalMarginProperty = DependencyProperty.Register(
    "OriginalMargin", typeof(Thickness), typeof(AnimatedButton), new PropertyMetadata(default(Thickness)));

        public Thickness? OriginalMargin
        {
            get { return (Thickness)GetValue(OriginalMarginProperty); }
            set { SetValue(OriginalMarginProperty, value); }
        }




        public event Action AnimationCompleted;
        public async Task AnimateSize(double? newWidth, double? newHeight, TimeSpan duration)
        {
            if (ActualHeight == newHeight && ActualWidth == newHeight) return;
            var tempW = ActualWidth;
            var tempH = ActualHeight;
            // Отмените предыдущую анимацию, если она запущена
            cts?.Cancel();
            cts = new CancellationTokenSource();

            // Вычислите шаги изменения размера
            double? widthStep = (newWidth - tempW) / duration.TotalMilliseconds;
            double? heightStep = (newHeight - tempH) / duration.TotalMilliseconds;

            // Создайте таймер для анимации
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed < duration)
            {
                // Если анимация была отменена, прекратите ее
                if (cts.Token.IsCancellationRequested)
                {
                    stopwatch.Stop();
                    return;
                }

                // Вычислите новые размеры
                double elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                this.Width = (double)(tempW + widthStep * elapsedMilliseconds);
                this.Height = (double)(tempH + heightStep * elapsedMilliseconds);

                // Обновите интерфейс
                await Task.Delay(1);
            }

            // Установите окончательные размеры
            this.Width = (double)newWidth;
            this.Height = (double)newHeight;

            stopwatch.Stop();
        }
        public async Task AnimateMargin(Thickness newMargin, TimeSpan duration)
        {
            if (Margin == newMargin) return;


            cts?.Cancel();
            cts = new CancellationTokenSource();



            double leftStep = (newMargin.Left - Margin.Left) / duration.TotalMilliseconds;
            double topStep = (newMargin.Top - Margin.Top) / duration.TotalMilliseconds;
            double rightStep = (newMargin.Right - Margin.Right) / duration.TotalMilliseconds;
            double bottomStep = (newMargin.Bottom - Margin.Bottom) / duration.TotalMilliseconds;


            var left = Margin.Left;
            var top = Margin.Top;
            var right = Margin.Right;
            var bottom = Margin.Bottom;


            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed < duration)
            {

                if (cts.Token.IsCancellationRequested)
                {
                    stopwatch.Stop();
                    return;
                }

                double elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                this.Margin = new Thickness(
                    left + leftStep * elapsedMilliseconds,
                    top + topStep * elapsedMilliseconds,
                    right + rightStep * elapsedMilliseconds,
                    bottom + bottomStep * elapsedMilliseconds
                );


                await Task.Delay(1);
            }

            this.Margin = newMargin;

            stopwatch.Stop();

        }

        public async Task HideButton()
        {
            if (OriginalHeight == null && OriginalWidth == null && OriginalMargin == null)
            {
                OriginalWidth = this.Width;
                OriginalHeight = this.Height;
                OriginalMargin = this.Margin;
            }
            await AnimateSize(0, 0, TimeSpan.FromSeconds(0.25));
            await AnimateMargin(new Thickness(0), TimeSpan.FromSeconds(0.25));
            AnimationCompleted?.Invoke();
        }

        public async Task ShowButton()
        {
            await AnimateSize(OriginalWidth, OriginalHeight, TimeSpan.FromSeconds(0.25));
            await AnimateMargin(OriginalMargin.Value, TimeSpan.FromSeconds(0.25));
            AnimationCompleted?.Invoke();
        }
    }
}