using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeImage
    {
        string imageSourceNow = null;
        Storyboard storyboard = null;
        Image imageControl = null;
        DispatcherQueue dispatcherQueue = null;
        string databaseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "photosCache");

        public AnimationsChangeImage(Image imageControl, DispatcherQueue dispatcherQueue)
        {
            this.imageControl = imageControl;
            this.dispatcherQueue = dispatcherQueue;
        }

        public void ChangeImageWithAnimation(string newImageSourceUrl)
        {
            if (imageSourceNow != null && imageSourceNow == newImageSourceUrl)
                return;
            imageSourceNow = newImageSourceUrl;

            dispatcherQueue.TryEnqueue(async () =>
            {
                if (storyboard == null) storyboard = new Storyboard();
                if (storyboard.GetCurrentState() == ClockState.Active)
                {
                    storyboard.Pause();
                }

                var animation = new DoubleAnimation
                {
                    From = imageControl.Opacity,
                    To = 0.0,
                    Duration = TimeSpan.FromMilliseconds(50),
                };

                storyboard = new Storyboard();
                Storyboard.SetTarget(animation, imageControl);
                Storyboard.SetTargetProperty(animation, "Opacity");

                storyboard.Children.Add(animation);

                storyboard.Completed += async (s, e) =>
                {
                    if (newImageSourceUrl == null || newImageSourceUrl == "null") return;
                    var bitmapImage = await GetImageAsync(newImageSourceUrl);

                    imageControl.Source = bitmapImage;

                    var animation = new DoubleAnimation
                    {
                        From = imageControl.Opacity,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(500),
                    };

                    storyboard = new Storyboard();
                    Storyboard.SetTarget(animation, imageControl);
                    Storyboard.SetTargetProperty(animation, "Opacity");

                    storyboard.Children.Add(animation);
                    storyboard.Begin();
                };

                storyboard.Begin();
            });
        }

        private async Task<BitmapImage> GetImageAsync(string newImageSourceUrl)
        {
            var fileName = Path.Combine(databaseFolderPath, GetHashString(newImageSourceUrl));

            if (File.Exists(fileName))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri(fileName);
                return bitmapImage;
            }
            else
            {
                var httpClient = new HttpClient();
                try
                {
                    var buffer = await httpClient.GetByteArrayAsync(newImageSourceUrl);

                    if (buffer != null && buffer.Length > 0)
                    {
                        await File.WriteAllBytesAsync(fileName, buffer);

                        var bitmapImage = new BitmapImage();
                        bitmapImage.UriSource = new Uri(fileName);
                        return bitmapImage;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    // Если произошла ошибка при загрузке или сохранении изображения, возвращаем null
                    return null;
                }
            }
        }


        private string GetHashString(string inputString)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(inputString));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
