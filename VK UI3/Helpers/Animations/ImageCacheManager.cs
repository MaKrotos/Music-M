using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using VK_UI3.DB;

namespace VK_UI3.Helpers.Animations
{
    public class ImageCacheManager
    {
        private readonly string _cacheFolderPath;
        private readonly DispatcherQueue _dispatcherQueue;
        private const int ClearFileThresholdMinutes = 5;
        private const int DefaultCacheSizeMb = 100;

        public ImageCacheManager(string cacheFolderName, DispatcherQueue dispatcherQueue)
        {
            // Формируем путь к папке кэша
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _cacheFolderPath = Path.Combine(appDataPath, "VKMMKZ", cacheFolderName);
            _dispatcherQueue = dispatcherQueue;

            if (!Directory.Exists(_cacheFolderPath))
            {
                Directory.CreateDirectory(_cacheFolderPath);
            }
        }

        /// <summary>
        /// Возвращает изображение из кэша по URI.
        /// Если файл является локальным или уже закеширован, возвращается BitmapImage.
        /// </summary>
        public async Task<BitmapImage> GetCachedImageAsync(Uri imageUri, bool setColorTheme = false)
        {
            if (imageUri == null)
                return null;

            // Если путь ведёт к локальному файлу – загружаем его напрямую
            if (imageUri.IsFile || File.Exists(imageUri.ToString()))
            {
                return await LoadBitmapImageFromUriAsync(imageUri).ConfigureAwait(false);
            }

            string cacheFilePath = GetCachedFilePath(imageUri);
            if (File.Exists(cacheFilePath))
            {
                return await LoadBitmapImageFromUriAsync(new Uri(cacheFilePath)).ConfigureAwait(false);
            }

            // Файл не найден в кэше
            return null;
        }

        /// <summary>
        /// Сохраняет изображение в кэш. При необходимости выполняет обработку (подгонку цвета под тему).
        /// </summary>
        public async Task SaveImageToCacheAsync(Uri imageUri, byte[] imageData, bool setColorTheme = false)
        {
            if (imageUri == null || imageData == null || imageData.Length == 0)
                return;

            string cacheFilePath = GetCachedFilePath(imageUri);
            await File.WriteAllBytesAsync(cacheFilePath, imageData).ConfigureAwait(false);

            if (setColorTheme)
            {
                ApplyColorThemeToImage(cacheFilePath);
            }

            // Асинхронно проверяем и удаляем устаревшие файлы из кэша
            _ = CheckAndDeleteOldFilesAsync();
        }

        /// <summary>
        /// Асинхронно загружает изображение через диспетчер UI.
        /// </summary>
        private Task<BitmapImage> LoadBitmapImageFromUriAsync(Uri uri)
        {
            var tcs = new TaskCompletionSource<BitmapImage>();

            _dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    var bitmap = new BitmapImage(uri);
                    tcs.SetResult(bitmap);
                }
                catch
                {
                    // При ошибке загрузки возвращаем null
                    tcs.SetResult(null);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Возвращает путь к файлу кэша на основе SHA256-хеша URI.
        /// </summary>
        private string GetCachedFilePath(Uri imageUri)
        {
            string hash = ComputeSHA256Hash(imageUri.ToString());
            return Path.Combine(_cacheFolderPath, hash);
        }

        /// <summary>
        /// Применяет цветовую тему к изображению: заменяет непрозрачные пиксели на черный (светлая тема)
        /// или белый (темная тема).
        /// </summary>
        public static void ApplyColorThemeToImage(string imagePath)
        {
            try
            {
                byte[] imageBytes = File.ReadAllBytes(imagePath);
                using (var ms = new MemoryStream(imageBytes))
                using (Bitmap originalBmp = new Bitmap(ms))
                using (Bitmap bmp = new Bitmap(originalBmp))
                {
                    Color themeColor = GetThemeContrastColor();

                    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

                    unsafe
                    {
                        byte* ptr = (byte*)data.Scan0;
                        int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                        int height = data.Height;
                        int widthInBytes = data.Width * bytesPerPixel;

                        Parallel.For(0, height, y =>
                        {
                            byte* row = ptr + (y * data.Stride);
                            for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                            {
                                int alpha = row[x + 3];
                                if (alpha != 0)
                                {
                                    row[x] = themeColor.B;
                                    row[x + 1] = themeColor.G;
                                    row[x + 2] = themeColor.R;
                                }
                            }
                        });
                    }

                    bmp.UnlockBits(data);
                    bmp.Save(imagePath, ImageFormat.Png);
                }
            }
            catch
            {
                // Ошибки обработки изображения игнорируются
            }
        }

        /// <summary>
        /// Определяет цвет для обработки изображения в зависимости от текущей темы.
        /// </summary>
        private static Color GetThemeContrastColor()
        {
            var currentTheme = Application.Current.RequestedTheme;
            return currentTheme == ApplicationTheme.Light ? Color.Black : Color.White;
        }

        /// <summary>
        /// Асинхронно проверяет размер кэша и удаляет устаревшие файлы, если размер превышает лимит.
        /// </summary>
        private async Task CheckAndDeleteOldFilesAsync()
        {
            await Task.Run(() =>
            {
                var lastClearSetting = SettingsTable.GetSetting("timeLastClear");
                if (lastClearSetting == null || !DateTime.TryParse(lastClearSetting.settingValue, out DateTime lastClearTime))
                {
                    SettingsTable.SetSetting("timeLastClear", DateTime.Now.ToString());
                    return;
                }

                if ((DateTime.Now - lastClearTime).TotalMinutes < ClearFileThresholdMinutes)
                    return;

                int cacheSizeMb = DefaultCacheSizeMb;
                var photoSizeCacheSetting = SettingsTable.GetSetting("photoCacheSize");
                if (photoSizeCacheSetting != null && int.TryParse(photoSizeCacheSetting.settingValue, out int sizeValue))
                {
                    cacheSizeMb = sizeValue;
                }

                long maxCacheSizeBytes = cacheSizeMb * 1024L * 1024L;
                if (Directory.Exists(_cacheFolderPath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(_cacheFolderPath);
                    FileInfo[] files = directoryInfo.GetFiles();
                    long currentCacheSize = files.Sum(f => f.Length);

                    if (currentCacheSize > maxCacheSizeBytes)
                    {
                        foreach (var file in files.OrderBy(f => f.CreationTime))
                        {
                            if ((DateTime.Now - file.CreationTime).TotalMinutes < ClearFileThresholdMinutes)
                                continue;

                            try
                            {
                                currentCacheSize -= file.Length;
                                file.Delete();
                            }
                            catch
                            {
                                // Если не удалось удалить файл – пропускаем
                            }

                            if (currentCacheSize <= maxCacheSizeBytes)
                                break;
                        }
                    }

                    SettingsTable.SetSetting("timeLastClear", DateTime.Now.ToString());
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Вычисляет SHA256-хеш от исходной строки.
        /// </summary>
        private string ComputeSHA256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                // Можно также использовать BitConverter, чтобы получить хеш в виде строки
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
