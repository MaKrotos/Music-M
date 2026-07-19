using DevWinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MusicX.Core.Models;
using System;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;

namespace VK_UI3.Views.Controls
{
    public sealed partial class ListeningContentItemControl : UserControl
    {
        private AnimationsChangeImage _animationsChangeImage;

        public ListeningContentItemControl()
        {
            this.InitializeComponent();
            this.Loaded += ListeningContentItemControl_Loaded;
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ApplyDataContext();
        }

        private void ApplyDataContext()
        {
            if (DataContext is ListeningContentItem item)
            {
                // Устанавливаем название из DataContext
                ItemNameText.Text = item.Name;

                // Если Avatar пустой — сразу показываем заглушку
                if (string.IsNullOrWhiteSpace(item.Avatar))
                {
                    AvatarImage.Visibility = Visibility.Collapsed;
                    FallbackIcon.Visibility = Visibility.Visible;
                    FallbackIcon.Glyph = item.Icon;
                }
                else
                {
                    // Загружаем обложку из Avatar через AnimationsChangeImage
                    AvatarImage.Visibility = Visibility.Visible;
                    FallbackIcon.Visibility = Visibility.Collapsed;
                    _animationsChangeImage?.ChangeImageWithAnimation(item.Avatar);
                }
            }
        }

        private void ListeningContentItemControl_Loaded(object sender, RoutedEventArgs e)
        {
            mainGrid.Lights.Add(new AmbLight());
            mainGrid.Lights.Add(new RippleLight());
            mainGrid.Lights.Add(new HoverLight());

            // Инициализируем после загрузки, когда DispatcherQueue доступен
            _animationsChangeImage = new AnimationsChangeImage(AvatarImage, this.DispatcherQueue);

            // Если DataContext уже установлен до Loaded — применяем аватар сейчас
            ApplyDataContext();
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (DataContext is ListeningContentItem item && !string.IsNullOrWhiteSpace(item.Url))
            {
                // Сначала пытаемся открыть через внутреннюю навигацию приложения
                if (VkNavigationHelper.TryNavigate(item))
                    return;

                // Если не получилось — открываем в браузере
                try
                {
                    var uri = new Uri(item.Url);
                    _ = Windows.System.Launcher.LaunchUriAsync(uri);
                }
                catch (UriFormatException)
                {
                    // Некорректный URL — игнорируем
                }
            }
        }

        private void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // Если изображение не загрузилось — показываем иконку-заглушку
            AvatarImage.Visibility = Visibility.Collapsed;
            FallbackIcon.Visibility = Visibility.Visible;
            if (DataContext is ListeningContentItem item)
            {
                FallbackIcon.Glyph = item.Icon;
            }
        }
    }
}