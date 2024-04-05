using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VK_UI3.Helpers.Animations
{
    public class AnimatedButton : Button
    {
        private double? originalWidth = null;
        private double? originalHeight = null;
        private Thickness? originalMargin = null;
        private CancellationTokenSource cts;

      
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
            var left = Margin.Left;
            var top = Margin.Top;
            var right = Margin.Right;
            var bottom = Margin.Bottom;

            double leftStep = (newMargin.Left - Margin.Left) / duration.TotalMilliseconds;
            double topStep = (newMargin.Top - Margin.Top) / duration.TotalMilliseconds;
            double rightStep = (newMargin.Right - Margin.Right) / duration.TotalMilliseconds;
            double bottomStep = (newMargin.Bottom - Margin.Bottom) / duration.TotalMilliseconds;

        
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
            if (originalHeight == null && originalWidth == null && originalMargin == null)
            {
                originalWidth = this.Width;
                originalHeight = this.Height;
                originalMargin = this.Margin;
            }
            await AnimateSize(0, 0, TimeSpan.FromSeconds(0.25));
            await AnimateMargin(new Thickness(0), TimeSpan.FromSeconds(0.25));
            AnimationCompleted?.Invoke();
        }

        public async Task ShowButton()
        {
            await AnimateSize(originalWidth, originalHeight, TimeSpan.FromSeconds(0.25));
            await AnimateMargin(originalMargin.Value, TimeSpan.FromSeconds(0.25));
            AnimationCompleted?.Invoke();
        }
    }
}