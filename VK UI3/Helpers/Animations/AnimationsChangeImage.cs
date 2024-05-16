using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeImage
    {
        string imageSourceNow = null;
        Storyboard storyboard = new Storyboard();
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
                return;
            else
                ChangeImageWithAnimation(newImageSourceUrl.ToString());
        }
        Object element = null;
        public void ChangeImageWithAnimation(string? newImageSourceUrl, bool justDoIt = false)
        {

            if (imageSourceNow != null && imageSourceNow == newImageSourceUrl && !justDoIt)
                return;
            if (newImageSourceUrl == null || newImageSourceUrl == "null") return;
            GetImageAsync(newImageSourceUrl);

            imageSourceNow = newImageSourceUrl;
            dispatcherQueue.TryEnqueue(async () =>
            {
                if (storyboard.GetCurrentState() == ClockState.Active)
                {
                    storyboard.Pause();
                }
  
                if (imageControl != null)
                    element = imageControl;
                if (imageBrushControl != null)
                    element = imageBrushControl;
                if (personPicture != null)
                    element = personPicture;
                if (imageIcon != null)
                    element = imageIcon;


                if ((element as FrameworkElement).Opacity == 0 || imageSourceNow == null)
                {
                    showImage((element as FrameworkElement));
                }
                else
                {
                    var animation = new DoubleAnimation
                    {
                        From = (element as FrameworkElement).Opacity,
                        To = 0.0,
                        Duration = TimeSpan.FromMilliseconds(250),
                    };

                    Storyboard.SetTarget(animation, (element as FrameworkElement));
                    Storyboard.SetTargetProperty(animation, "Opacity");
                    storyboard.Stop();
                    storyboard.Children.Clear();
                    storyboard.Children.Add(animation);

                    EventHandler<object> storyboardCompletedHandler = null;
                    storyboardCompletedHandler = async (s, e) =>
                    {
                        // Отписка от события после его выполнения
                        storyboard.Completed -= storyboardCompletedHandler;

                        showImage(element as FrameworkElement);


                    };
                    storyboard.Completed -= storyboardCompletedHandler;
                    storyboard.Completed += storyboardCompletedHandler;

                    storyboard.Begin();
                }
            });
        }

        private void showImage(FrameworkElement element)
        {
        

            if (imageControl != null)
            {
                imageControl.Source = image;
            }
            if (imageBrushControl != null)
            {
                imageBrushControl.ImageSource = image;
            }
            if (personPicture != null)
            {
                personPicture.ProfilePicture = image;
            }
            if (imageIcon != null)
            {
                imageIcon.Source = image;
            }

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
        BitmapImage image = null;
        private async Task<BitmapImage> GetImageAsync(string newImageSourceUrl)
        {
           
            var fileName = Path.Combine(databaseFolderPath, GetHashString(newImageSourceUrl));

            if (!Directory.Exists(databaseFolderPath)) Directory.CreateDirectory(databaseFolderPath);

            if (new Uri(newImageSourceUrl).IsFile)
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri(newImageSourceUrl);
                image = bitmapImage;
            
                return bitmapImage;
            }
            else if (File.Exists(fileName))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri(fileName);
                image = bitmapImage;
             
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
                        image = bitmapImage;
                     
                        return bitmapImage;
                    }
                    else
                    {
                        image = null;
                
                        return null;
                    }
                }
                catch (Exception e)
                {
                    image = null;
               
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
