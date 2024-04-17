using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Windows.Foundation;

namespace VK_UI3.Helpers.Animations
{
    class Palka : Grid
    {
        Storyboard storyboard = new Storyboard();
        private ScaleTransform scaleTransform = new ScaleTransform();
        DoubleAnimation scaleYAnimation;

        private Random random = new Random();
        public double Max { get; set; } = 2;
        public double Min { get; set; } = 0.3;
        public double Seconds { get; set; } = 0.25;

        public Palka(double max = 2, double min = 0.3, double seconds = 0.25)
        {
            this.Max = max;
            this.Min = min;
            this.Seconds = seconds;
            init();
        }
        public Palka()
        {
            init();
        }

        private void init()
        {
            scaleYAnimation = new DoubleAnimation()
            {
                To = random.NextDouble() * (Max - Min) + Min,
                Duration = new Duration(TimeSpan.FromSeconds(Seconds))
            };


            Storyboard.SetTarget(scaleYAnimation, scaleTransform);

            Storyboard.SetTargetProperty(scaleYAnimation, "ScaleY");
            storyboard.Children.Add(scaleYAnimation);
            storyboard.Completed += Storyboard_Completed;
            this.RenderTransform = scaleTransform;
            this.RenderTransformOrigin = new Point(0.5, 0.5);
        }



        private void Storyboard_Completed(object sender, object e)
        {

            // Устанавливаем случайный размер высоты
            scaleYAnimation.To = random.NextDouble() * (Max - Min) + Min;
            scaleYAnimation.Duration = new Duration(TimeSpan.FromSeconds(Seconds));
            // Запускаем анимацию снова
            storyboard.Begin();
        }

        public void StartAnimation()
        {
            storyboard.Begin();
        }
        public void StopAnimation()
        {
            storyboard.Pause();
            scaleYAnimation.To = 1;
            storyboard.Begin();
        }
    }
}
