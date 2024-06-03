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
using vkPosterBot.DB;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeImage
    {
        Uri imageSourceNow = null;
        Storyboard storyboard = new Storyboard();
        Microsoft.UI.Xaml.Controls.Image imageControl = null;
        ImageIcon imageIcon = null;
        ImageBrush imageBrushControl = null;
        PersonPicture personPicture = null;
        DispatcherQueue dispatcherQueue = null;

        string databaseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "photosCache");

        public AnimationsChangeImage(Microsoft.UI.Xaml.Controls.Image imageControl, DispatcherQueue dispatcherQueue)
        {
            element = imageControl;
            this.imageControl = imageControl;
            this.dispatcherQueue = dispatcherQueue;
        }


        public AnimationsChangeImage(ImageIcon imageIcon, DispatcherQueue dispatcherQueue)
        {
            element = imageIcon;
            this.imageIcon = imageIcon;
            this.dispatcherQueue = dispatcherQueue;
        }

        public AnimationsChangeImage(PersonPicture personPicture, DispatcherQueue dispatcherQueue)
        {
            element = personPicture;
            this.personPicture = personPicture;
            this.dispatcherQueue = dispatcherQueue;
        }

        public AnimationsChangeImage(ImageBrush imageBrushControl, DispatcherQueue dispatcherQueue)
        {
            element = imageBrushControl;
            this.imageBrushControl = imageBrushControl;
            this.dispatcherQueue = dispatcherQueue;
        }
        public void ChangeImageWithAnimation(string newImageSourceUrl, bool justDoIt = false, bool setColorTheme = false)
        {
            switch (newImageSourceUrl)
            {
                case null:
                    return;
                case "null":
                    dispatcherQueue.TryEnqueue(async () =>
                    {
                        hideAnimation(null);
                    });
                    break;
                default:
                    ChangeImageWithAnimation(new Uri(newImageSourceUrl), justDoIt, setColorTheme: setColorTheme);
                    break;
            }
        }
        Object element = null;
        public void ChangeImageWithAnimation(Uri? newImageSourceUrl, bool justDoIt = false, bool setColorTheme = false)
        {

            if (imageSourceNow != null && imageSourceNow == newImageSourceUrl && !justDoIt)
                return;

            Task<BitmapImage> GetImageTask = null ;
           
                GetImageTask = GetImageAsync(newImageSourceUrl, setColorTheme);
           
         
            imageSourceNow = newImageSourceUrl;
            dispatcherQueue.TryEnqueue(async () =>
            {
                if (storyboard.GetCurrentState() == ClockState.Active)
                {
                    storyboard.Pause();
                }
  
                

                if ((element as FrameworkElement).Opacity == 0 || imageSourceNow == null)
                {
                    showImage((element as FrameworkElement), await GetImageTask);
                }
                else
                {
                    hideAnimation(GetImageTask);
                }
            });
        }

        private void hideAnimation(Task<BitmapImage>? getImageTask)
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

            if (getImageTask != null)
            {
                storyboardCompletedHandler = async (s, e) =>
                {
                    // Отписка от события после его выполнения
                    storyboard.Completed -= storyboardCompletedHandler;
                    if (image != null)
                        showImage(element as FrameworkElement, await getImageTask);
                };
                storyboard.Completed -= storyboardCompletedHandler;
                storyboard.Completed += storyboardCompletedHandler;
            }
            storyboard.Begin();
        }

        private void showImage(FrameworkElement element, BitmapImage image)
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
        private async Task<BitmapImage> GetImageAsync(Uri newImageSourceUrl, bool setColorTheme)
        {

            if (newImageSourceUrl == null)
            {
                image = null;
                return null;
            }
            var fileName = Path.Combine(databaseFolderPath, GetHashString(newImageSourceUrl.ToString()));
            var tempFileName = fileName + "temp";

            if (!Directory.Exists(databaseFolderPath)) Directory.CreateDirectory(databaseFolderPath);
            BitmapImage bitmap = null;
            if (newImageSourceUrl.IsFile)
            {
                var tcs = new TaskCompletionSource<BitmapImage>();
                dispatcherQueue.TryEnqueue(() =>
                {
                    bitmap = new BitmapImage(new Uri(fileName));
                    image = bitmap;
                    tcs.SetResult(bitmap);
                });
                return await tcs.Task;
            }
            else if (File.Exists(fileName))
            {
                var tcs = new TaskCompletionSource<BitmapImage>();
                dispatcherQueue.TryEnqueue(() =>
                {
                    bitmap = new BitmapImage(new Uri(fileName));
                    image = bitmap;
                    tcs.SetResult(bitmap);
                });
                return await tcs.Task;
            }
            else
            {
                var httpClient = new HttpClient();
                try
                {
                    var buffer = await httpClient.GetByteArrayAsync(newImageSourceUrl);

                    if (buffer != null && buffer.Length > 0)
                    {
                        await File.WriteAllBytesAsync(tempFileName, buffer);
                        if (setColorTheme)
                            ColorOpaquePartFastParallel(tempFileName);
                        var tcs = new TaskCompletionSource<BitmapImage>();
                        File.Move(tempFileName, fileName);
                        dispatcherQueue.TryEnqueue(() =>
                        {
                            bitmap = new BitmapImage(new Uri(fileName));
                            image = bitmap;
                            tcs.SetResult(bitmap);
                        });
                       

                        _ = CheckAndDeleteOldFilesAsync(databaseFolderPath);
                        return await tcs.Task;
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

        public static void ColorOpaquePartFastParallel(string imagePath)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(imagePath)))
                {
                    Bitmap originalBmp = new Bitmap(ms);

                    // Загружаем изображение


                    // Создаем новый объект Bitmap из исходного изображения
                    Bitmap bmp = new Bitmap(originalBmp);

                    // Получаем текущую тему Windows
                    var currentTheme = Application.Current.RequestedTheme;

                    Color color;
                    if (currentTheme == ApplicationTheme.Light)
                    {
                        color = Color.Black; // Используйте нужный цвет для темной темы
                    }
                    else
                    {
                        color = Color.White; // Используйте нужный цвет для светлой темы
                    }

                    BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                    unsafe
                    {
                        byte* ptr = (byte*)data.Scan0;
                        int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                        int heightInPixels = data.Height;
                        int widthInBytes = data.Width * bytesPerPixel;
                        Parallel.For(0, heightInPixels, y =>
                        {
                            byte* currentLine = ptr + (y * data.Stride);
                            for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                            {
                                // Проверяем альфа-канал
                                int alphaByte = currentLine[x + 3];
                                if (alphaByte != 0) // Если пиксель непрозрачный
                                {
                                    // Закрашиваем его цветом, соответствующим текущей теме Windows
                                    currentLine[x] = color.B;
                                    currentLine[x + 1] = color.G;
                                    currentLine[x + 2] = color.R;
                                }
                            }
                        });
                    }
                    bmp.UnlockBits(data);

                    // Сохраняем изображение по тому же пути
                    bmp.Save(imagePath, ImageFormat.Png);

                    // Освобождаем ресурсы
                    originalBmp.Dispose();
                    bmp.Dispose();
                }
            }
            catch (Exception ex) 
            {
            
            
            }
        }




        private async Task CheckAndDeleteOldFilesAsync(string directoryPath)
        {
            _ = Task.Run(async() =>
            {
               var timeLastSetting= SettingsTable.GetSetting("timeLastClear");
                if (timeLastSetting == null)
                {
               
                    SettingsTable.SetSetting("timeLastClear", DateTime.Now.ToString());
                    return;
                }

                DateTime timeLast = DateTime.Parse(timeLastSetting.settingValue);
                if ((DateTime.Now - timeLast).TotalMinutes < 5)
                {
                    return;
                }



                int val = 100;
                var photoSizeCache = SettingsTable.GetSetting("photoCacheSize");
                if (photoSizeCache != null) val = int.Parse(photoSizeCache.settingValue);

                long maxDirectorySize = val * 1024 * 1024;
                if (Directory.Exists(directoryPath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
                    FileInfo[] files = directoryInfo.GetFiles();
                    long directorySize = files.Sum(file => file.Length);

                    if (directorySize > maxDirectorySize)
                    {
                        var orderedFiles = files.OrderBy(file => file.CreationTime);
                        foreach (var file in orderedFiles)
                        {
                            
                            directorySize -= file.Length;
                            file.Delete();
                            if (directorySize <= maxDirectorySize)
                            {
                                break;
                            }
                        }
                    }

                    SettingsTable.SetSetting("timeLastClear", DateTime.Now.ToString());
                }
                else
                {

                }
            });
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
