using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Net.Http;
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
        // Делегат, устанавливающий новый источник изображения для конкретного элемента
        private readonly Action<BitmapImage> _setImageSourceAction;

        /// <summary>
        /// Если элемент может быть приведён к FrameworkElement – используем его для анимации.
        /// </summary>
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

        /// <summary>
        /// Меняет изображение с анимацией. Принимает строку URL.
        /// Если передана строка "null" или null, осуществляется анимация скрытия.
        /// </summary>
        public void ChangeImageWithAnimation(string newImageSourceUrl, bool forceUpdate = false, bool setColorTheme = false)
        {
            if (string.IsNullOrEmpty(newImageSourceUrl) || newImageSourceUrl == "null")
            {
                _dispatcherQueue.TryEnqueue(() => HideAnimation(null));
                return;
            }

            ChangeImageWithAnimation(new Uri(newImageSourceUrl), forceUpdate, setColorTheme);
        }

        /// <summary>
        /// Меняет изображение с анимационным переходом.
        /// </summary>
        public async void ChangeImageWithAnimation(Uri newImageSource, bool forceUpdate = false, bool setColorTheme = false)
        {
            // Если новое изображение совпадает с текущим и не задан принудительный режим – выходим
            if (_currentImageSource != null && _currentImageSource.Equals(newImageSource) && !forceUpdate)
                return;

            _currentImageSource = newImageSource;

            var cachedImage = await _cacheManager.GetCachedImageAsync(newImageSource, setColorTheme);
            if (cachedImage != null)
            {
                UpdateImage(cachedImage);
                return;
            }

            await DownloadAndSetImage(newImageSource, setColorTheme);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Скачивает изображение по указанному URI, сохраняет его в кэш и обновляет изображение.
        /// </summary>
        private async Task DownloadAndSetImage(Uri imageUri, bool setColorTheme)
        {
            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) })
            {
                try
                {
                    var buffer = await httpClient.GetByteArrayAsync(imageUri);
                    if (buffer == null || buffer.Length == 0)
                        return;

                    await _cacheManager.SaveImageToCacheAsync(imageUri, buffer, setColorTheme);
                    var cachedImage = await _cacheManager.GetCachedImageAsync(imageUri, setColorTheme);
                    if (cachedImage != null)
                    {
                        UpdateImage(cachedImage);
                    }
                }
                catch
                {
                    // Ошибки загрузки можно залогировать при необходимости
                }
            }
        }

        /// <summary>
        /// Обновляет изображение с анимацией перехода.
        /// </summary>
        private void UpdateImage(BitmapImage image)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                // При активной анимации приостанавливаем storyboard
                if (_storyboard.GetCurrentState() == ClockState.Active)
                {
                    _storyboard.Pause();
                }

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

        /// <summary>
        /// Производит анимацию скрытия (уменьшения прозрачности до 0) с опциональным переходом на новое изображение.
        /// </summary>
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

        /// <summary>
        /// Устанавливает новый источник изображения и производит анимацию появления.
        /// </summary>
        private void ShowImage(BitmapImage image)
        {
            _setImageSourceAction?.Invoke(image);

            if (AnimatableElement != null)
            {
                AnimateOpacity(AnimatableElement.Opacity, 1.0, 500, null);
            }
        }

        /// <summary>
        /// Унифицированный метод для анимации изменения прозрачности элемента.
        /// Принимает начальное и конечное значения, длительность и опциональный обработчик завершения.
        /// </summary>
        private void AnimateOpacity(double from, double to, double durationMs, Action onCompleted)
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMs)
            };

            // Если AnimatableElement не определён (например, если исходный элемент не является FrameworkElement),
            // анимация не выполняется.
            if (AnimatableElement != null)
            {
                Storyboard.SetTarget(animation, AnimatableElement);
                Storyboard.SetTargetProperty(animation, "Opacity");
            }
            else
            {
                // Применяем анимацию сразу, если нет возможности анимировать
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
