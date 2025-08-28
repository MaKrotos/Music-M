using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeImage
    {
        private Uri _currentImageSource = null;
        private readonly Storyboard _storyboard = new Storyboard();
        private readonly object _element;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ImageCacheManager _cacheManager;
        private readonly Action<BitmapImage> _setImageSourceAction;

        // Статическая очередь для управления потоками загрузки
        private static SemaphoreSlim _downloadSemaphore;
        private static readonly ConcurrentDictionary<Uri, Task<BitmapImage>> _activeDownloads = new();

        // Настройки по умолчанию - можно изменить через статические методы
        private static int _maxConcurrentDownloads = 10;
        private static bool _enableQueue = true;

        static AnimationsChangeImage()
        {
            _downloadSemaphore = new SemaphoreSlim(_maxConcurrentDownloads, _maxConcurrentDownloads);
        }

        /// <summary>
        /// Устанавливает максимальное количество одновременных загрузок
        /// </summary>
        public static void SetMaxConcurrentDownloads(int maxDownloads)
        {
            if (maxDownloads < 1)
                maxDownloads = 1;
            if (_maxConcurrentDownloads != maxDownloads)
            {
                _maxConcurrentDownloads = maxDownloads;
                _downloadSemaphore.Dispose();
                _downloadSemaphore = new SemaphoreSlim(maxDownloads, maxDownloads);
            }
        }

        /// <summary>
        /// Включает или выключает систему очередей
        /// </summary>
        public static void EnableQueueSystem(bool enable)
        {
            _enableQueue = enable;
        }

        private FrameworkElement AnimatableElement => _element as FrameworkElement;

        #region Конструкторы

        public AnimationsChangeImage(Image imageControl, DispatcherQueue dispatcherQueue)
        {
            _element = imageControl;
            _dispatcherQueue = dispatcherQueue;
            _cacheManager = new ImageCacheManager("photosCache", dispatcherQueue);
            _setImageSourceAction = image => imageControl.Source = image;
        }

        public AnimationsChangeImage(ImageIcon imageIcon, DispatcherQueue dispatcherQueue)
        {
            _element = imageIcon;
            _dispatcherQueue = dispatcherQueue;
            _cacheManager = new ImageCacheManager("photosCache", dispatcherQueue);
            _setImageSourceAction = image => imageIcon.SetValue(ImageIcon.SourceProperty, image);
        }

        public AnimationsChangeImage(PersonPicture personPicture, DispatcherQueue dispatcherQueue)
        {
            _element = personPicture;
            _dispatcherQueue = dispatcherQueue;
            _cacheManager = new ImageCacheManager("photosCache", dispatcherQueue);
            _setImageSourceAction = image => personPicture.SetValue(PersonPicture.ProfilePictureProperty, image);
        }

        public AnimationsChangeImage(ImageBrush imageBrushControl, DispatcherQueue dispatcherQueue)
        {
            _element = imageBrushControl;
            _dispatcherQueue = dispatcherQueue;
            _cacheManager = new ImageCacheManager("photosCache", dispatcherQueue);
            _setImageSourceAction = image => imageBrushControl.SetValue(ImageBrush.ImageSourceProperty, image);
        }

        #endregion

        #region Public API

        public void ChangeImageWithAnimation(string newImageSourceUrl, bool forceUpdate = false, bool setColorTheme = false)
        {
            if (string.IsNullOrEmpty(newImageSourceUrl) || newImageSourceUrl == "null")
            {
                _dispatcherQueue.TryEnqueue(() => HideAnimation(null));
                return;
            }

            ChangeImageWithAnimation(new Uri(newImageSourceUrl), forceUpdate, setColorTheme);
        }

        public async void ChangeImageWithAnimation(Uri newImageSource, bool forceUpdate = false, bool setColorTheme = false)
        {
            if (newImageSource == null)
            {
                _dispatcherQueue.TryEnqueue(() => HideAnimation(null));
                return;
            }

            if (_currentImageSource != null && _currentImageSource.Equals(newImageSource) && !forceUpdate)
                return;

            _currentImageSource = newImageSource;

            // Для локальных файлов не проверяем кэш
            if (IsLocalFile(newImageSource))
            {
                var localImage = await LoadLocalImage(newImageSource);
                if (localImage != null)
                {
                    UpdateImage(localImage);
                }
                return;
            }

            // Проверяем кэш только для HTTP URI
            var cachedImage = await _cacheManager.GetCachedImageAsync(newImageSource, setColorTheme);
            if (cachedImage != null)
            {
                UpdateImage(cachedImage);
                return;
            }

            // Если изображения нет в кэше - начинаем загрузку
            await ProcessImageDownload(newImageSource, setColorTheme);
        }

        #endregion

        #region Private Methods

        private async Task ProcessImageDownload(Uri imageUri, bool setColorTheme)
        {
            // Проверяем, является ли URI локальным файлом
            if (IsLocalFile(imageUri))
            {
                // Для локальных файлов не используем очередь и кэш
                var localImage = await LoadLocalImage(imageUri);
                if (localImage != null && _currentImageSource == imageUri)
                {
                    UpdateImage(localImage);
                }
                return;
            }

            // Проверяем, не загружается ли уже это изображение в другом потоке (только для HTTP)
            if (_activeDownloads.TryGetValue(imageUri, out var existingDownload))
            {
                try
                {
                    var image = await existingDownload;
                    if (image != null && _currentImageSource == imageUri)
                    {
                        UpdateImage(image);
                    }
                }
                catch
                {
                    // Ошибка уже обработана в основном потоке загрузки
                }
                return;
            }

            // Создаем новую задачу загрузки (только для HTTP)
            var downloadTask = DownloadAndSetImage(imageUri, setColorTheme);
            _activeDownloads.TryAdd(imageUri, downloadTask);

            try
            {
                await downloadTask;
            }
            finally
            {
                _activeDownloads.TryRemove(imageUri, out _);
            }
        }

        private async Task<BitmapImage> DownloadAndSetImage(Uri imageUri, bool setColorTheme)
        {
            // Для локальных файлов этот метод не должен вызываться
            if (IsLocalFile(imageUri))
            {
                return await LoadLocalImage(imageUri);
            }

            if (_enableQueue)
            {
                await _downloadSemaphore.WaitAsync();
            }

            try
            {
                using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) })
                {
                    var buffer = await httpClient.GetByteArrayAsync(imageUri);
                    if (buffer == null || buffer.Length == 0)
                        return null;

                    await _cacheManager.SaveImageToCacheAsync(imageUri, buffer, setColorTheme);
                    var cachedImage = await _cacheManager.GetCachedImageAsync(imageUri, setColorTheme);

                    if (cachedImage != null && _currentImageSource == imageUri)
                    {
                        _dispatcherQueue.TryEnqueue(() => UpdateImage(cachedImage));
                    }

                    return cachedImage;
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (_enableQueue)
                {
                    _downloadSemaphore.Release();
                }
            }
        }

        private bool IsLocalFile(Uri uri)
        {
            return uri.IsFile ||
                   uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase) ||
                   !uri.IsAbsoluteUri ||
                   string.IsNullOrEmpty(uri.Host);
        }

        private Task<BitmapImage> LoadLocalImage(Uri fileUri)
        {
            var tcs = new TaskCompletionSource<BitmapImage>();

            _dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    string filePath = fileUri.IsFile ? fileUri.LocalPath : fileUri.ToString();

                    if (!File.Exists(filePath))
                    {
                        tcs.SetResult(null);
                        return;
                    }

                    var bitmapImage = new BitmapImage();
                    bitmapImage.UriSource = fileUri;
                    tcs.SetResult(bitmapImage);
                }
                catch (Exception ex)
                {
                    tcs.SetResult(null);
                }
            });

            return tcs.Task;
        }
        private void UpdateImage(BitmapImage image)
        {
            this._dispatcherQueue.TryEnqueue(() =>
            {
                if (_storyboard != null)
                    _storyboard.Pause();


                if (AnimatableElement == null || AnimatableElement.Opacity == 0 || _currentImageSource == null)
                {
                    if (AnimatableElement != null)
                        AnimatableElement.Opacity = 0;
                    ShowImage(image);
                }
                else
                {
                    HideAnimation(image);
                }
            });
        }

        private void HideAnimation(BitmapImage newImage)
        {
            AnimateOpacity(AnimatableElement?.Opacity ?? 1.0, 0.0, 250, () =>
            {
                if (newImage != null)
                {
                    ShowImage(newImage);
                }
            });
        }

        private void ShowImage(BitmapImage image)
        {
            _setImageSourceAction?.Invoke(image);

            if (AnimatableElement != null)
            {
                AnimateOpacity(AnimatableElement.Opacity, 1.0, 500, null);
            }
        }

        private void AnimateOpacity(double from, double to, double durationMs, Action onCompleted)
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMs)
            };

            if (AnimatableElement != null)
            {
                Storyboard.SetTarget(animation, AnimatableElement);
                Storyboard.SetTargetProperty(animation, "Opacity");
            }
            else
            {
                onCompleted?.Invoke();
                return;
            }

            _storyboard.Stop();
            _storyboard.Children.Clear();

            if (onCompleted != null)
            {
                EventHandler<object> handler = null;
                handler = (s, e) =>
                {
                    _storyboard.Completed -= handler;
                    onCompleted.Invoke();
                };
                _storyboard.Completed += handler;
            }

            _storyboard.Children.Add(animation);
            _storyboard.Begin();
        }

        #endregion
    }
}