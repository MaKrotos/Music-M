using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeImage
    {
        string imageSourceNow = null;
        Storyboard storyboard = null;
        Image imageControl = null;
        ImageIcon imageIcon = null;
        ImageBrush imageBrushControl = null;
        PersonPicture personPicture = null;
        DispatcherQueue dispatcherQueue = null;

        string databaseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "photosCache");

        public AnimationsChangeImage(Image imageControl, DispatcherQueue dispatcherQueue)
        {
            this.imageControl = imageControl;
            this.dispatcherQueue = dispatcherQueue;
        }


        public AnimationsChangeImage(ImageIcon imageIcon, DispatcherQueue dispatcherQueue)
        {
            this.imageIcon = imageIcon;
            this.dispatcherQueue = dispatcherQueue;
        }

        public AnimationsChangeImage(PersonPicture personPicture, DispatcherQueue dispatcherQueue)
        {
            this.personPicture = personPicture;
            this.dispatcherQueue = dispatcherQueue;
        }

        public AnimationsChangeImage(ImageBrush imageBrushControl, DispatcherQueue dispatcherQueue)
        {
            this.imageBrushControl = imageBrushControl;
            this.dispatcherQueue = dispatcherQueue;
        }
        public void ChangeImageWithAnimation(Uri newImageSourceUrl)
        {
            if (newImageSourceUrl == null) 
                ChangeImageWithAnimation((string)null);
            else
                ChangeImageWithAnimation(newImageSourceUrl.ToString());
        }

        public void ChangeImageWithAnimation(string? newImageSourceUrl, bool justDoIt = false)
        {
            if (imageSourceNow != null && imageSourceNow == newImageSourceUrl && !justDoIt)
                return;
            imageSourceNow = newImageSourceUrl;

            dispatcherQueue.TryEnqueue(async () =>
            {
                if (storyboard == null) storyboard = new Storyboard();
                if (storyboard.GetCurrentState() == ClockState.Active)
                {
                    storyboard.Pause();
                }
                Object element = null;
                if (imageControl != null)
                    element = imageControl;
                if (imageBrushControl != null)
                    element = imageBrushControl;
                if (personPicture != null)
                    element = personPicture;
                if (imageIcon != null)
                    element = imageIcon;

                var animation = new DoubleAnimation
                {
                    From = (element as FrameworkElement).Opacity,
                    To = 0.0,
                    Duration = TimeSpan.FromMilliseconds(250),
                };

                storyboard = new Storyboard();
                Storyboard.SetTarget(animation, (element as FrameworkElement));
                Storyboard.SetTargetProperty(animation, "Opacity");

                storyboard.Children.Add(animation);

                storyboard.Completed += async (s, e) =>
                {
                    if (newImageSourceUrl == null || newImageSourceUrl == "null") return;
                    var bitmapImage = await GetImageAsync(newImageSourceUrl);

                    if (imageControl != null)
                    {
                        imageControl.Source = bitmapImage;
                    }
                    if (imageBrushControl != null)
                    {
                        imageBrushControl.ImageSource = bitmapImage;
                    }
                    if (personPicture != null)
                    {
                        personPicture.ProfilePicture = bitmapImage;
                    }
                    if (imageIcon != null)
                    {
                        imageIcon.Source = bitmapImage;
                    }
                    

                    var animation = new DoubleAnimation
                    {
                        From = (element as FrameworkElement).Opacity,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(500),
                    };

                    storyboard = new Storyboard();
                    Storyboard.SetTarget(animation, (element as FrameworkElement));
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

            if (!Directory.Exists(databaseFolderPath)) Directory.CreateDirectory(databaseFolderPath);

            if (new Uri(newImageSourceUrl).IsFile)
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri(newImageSourceUrl);
                return bitmapImage;
            }
            else if (File.Exists(fileName))
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
                catch (Exception e)
                {
                    AppCenterHelper.SendCrash(e);
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
