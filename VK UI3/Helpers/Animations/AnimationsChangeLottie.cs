using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using VK_UI3.DB;
using CommunityToolkit.WinUI.Lottie;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeLottie
    {
        Uri imageSourceNow = null;
        Storyboard storyboard = new Storyboard();
        Grid imageControl = null;
        LottieVisualSource lottieVisualSource = null;
        DispatcherQueue dispatcherQueue = null;

        string databaseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "photosCache");

        public AnimationsChangeLottie(Grid imageControl, LottieVisualSource lottieVisualSource, DispatcherQueue dispatcherQueue)
        {
            this.lottieVisualSource = lottieVisualSource;
            this.imageControl = imageControl;
            this.dispatcherQueue = dispatcherQueue;
        }


      

        public void ChangeImageWithAnimation(string newImageSourceUrl, bool justDoIt = false)
        {
            switch (newImageSourceUrl)
            {
                case null:
                    dispatcherQueue.TryEnqueue(async () =>
                    {
                        hideAnimation();
                    });
                    return;
                case "null":
                    dispatcherQueue.TryEnqueue(async () =>
                    {
                        hideAnimation();
                    });
                    break;
                default:
                    ChangeImageWithAnimation(new Uri(newImageSourceUrl), justDoIt);
                    break;
            }
        }
 
        public void ChangeImageWithAnimation(Uri? newImageSourceUrl, bool justDoIt = false)
        {

            if (imageSourceNow != null && imageSourceNow.Equals(newImageSourceUrl) && !justDoIt)
                return;
         
            imageSourceNow = newImageSourceUrl;
            dispatcherQueue.TryEnqueue(async () =>
            {
                if (storyboard.GetCurrentState() == ClockState.Active)
                {
                    storyboard.Pause();
                }
  
                

                if (imageControl.Opacity == 0 || imageSourceNow == null)
                {
                    imageControl.Opacity = 0;
                    showImage(imageControl);
                }
                else
                {
                    hideAnimation();
                }
            });
        }

        private void hideAnimation()
        {
            var animation = new DoubleAnimation
            {
                From = imageControl.Opacity,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(250),
            };

            Storyboard.SetTarget(animation, imageControl);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Stop();
            storyboard.Children.Clear();
            storyboard.Children.Add(animation);


            EventHandler<object> storyboardCompletedHandler = null;

            if (imageSourceNow != null)
            {
                storyboardCompletedHandler = async (s, e) =>
                {
                    // Отписка от события после его выполнения
                    storyboard.Completed -= storyboardCompletedHandler;
                        showImage(imageControl);
                };
                storyboard.Completed -= storyboardCompletedHandler;
                storyboard.Completed += storyboardCompletedHandler;
            }
            storyboard.Begin();
        }



        private async void showImage(FrameworkElement element)
        {
            await lottieVisualSource.SetSourceAsync(imageSourceNow);

         
            var animation = new DoubleAnimation
            {
                From = (element).Opacity,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
            };

            Storyboard.SetTarget(animation, (element));
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Stop();
            storyboard.Children.Clear();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
      

        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private ConcurrentDictionary<Uri, SemaphoreSlim> _urlSemaphores = new ConcurrentDictionary<Uri, SemaphoreSlim>();
    
    }
}
