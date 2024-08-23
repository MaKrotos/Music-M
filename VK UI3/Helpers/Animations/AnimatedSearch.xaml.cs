using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using VK_UI3.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Helpers.Animations
{
    public sealed partial class AnimatedSearch : Grid
    {
        public AnimatedSearch()
        {
            this.InitializeComponent();
        }


      

        public static readonly DependencyProperty OriginalHeightProperty = DependencyProperty.Register(
            "OriginalHeight", typeof(double), typeof(AnimatedButton), new PropertyMetadata(default(double)));

        public double? OriginalHeight
        {
            get { return (double)GetValue(OriginalHeightProperty); }
            set { SetValue(OriginalHeightProperty, value); }
        }




        private CancellationTokenSource cts;
        public async Task AnimateSize(double? newHeight, TimeSpan duration)
        {
            if (ActualHeight == newHeight && ActualWidth == newHeight) return;
            var tempW = ActualWidth;
            var tempH = ActualHeight;
            // Отмените предыдущую анимацию, если она запущена
            cts?.Cancel();
            cts = new CancellationTokenSource();

            // Вычислите шаги изменения размера
           
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
              
                this.Height = (double)(tempH + heightStep * elapsedMilliseconds);

                // Обновите интерфейс
                await Task.Delay(1);
            }

            // Установите окончательные размеры
       
            this.Height = (double)newHeight;

            stopwatch.Stop();
        }


        public async Task Hide()
        {
            if (OriginalHeight == null)
            {
                
                OriginalHeight = this.Height;
            }
            await AnimateSize(0, TimeSpan.FromSeconds(0.25));
           // await AnimateMargin(new Thickness(0), TimeSpan.FromSeconds(0.25));
           // AnimationCompleted?.Invoke();
        }

        public async Task Show()
        {
            await AnimateSize(OriginalHeight, TimeSpan.FromSeconds(0.25));
           // await AnimateMargin(OriginalMargin.Value, TimeSpan.FromSeconds(0.25));
           // AnimationCompleted?.Invoke();
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            MainView.OpenSearchSection(searchTXT.Text);
        }
    }
}
