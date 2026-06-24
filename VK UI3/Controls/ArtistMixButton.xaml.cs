using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VK_UI3.Helpers.Animations;
using VK_UI3.VKs.IVK;
using Windows.UI;

namespace VK_UI3.Controls
{
    public sealed partial class ArtistMixButton : UserControl
    {
        private MusicX.Core.Models.Button _button;
        private AnimationsChangeText _titleAnim;
        private AnimationsChangeText _descriptionAnim;
        private AnimationsChangeFontIcon _animationsChangeFontIcon;

        public ArtistMixButton()
        {
            this.InitializeComponent();
            this.Loaded += ArtistMixButton_Loaded;
            this.Unloaded += ArtistMixButton_Unloaded;
            this.DataContextChanged += ArtistMixButton_DataContextChanged;
        }

        private void ArtistMixButton_Loaded(object sender, RoutedEventArgs e)
        {
            // Инициализация анимаций
            _titleAnim = new AnimationsChangeText(TitleText, this.DispatcherQueue);
            _descriptionAnim = new AnimationsChangeText(DescriptionText, this.DispatcherQueue);
            _animationsChangeFontIcon = new AnimationsChangeFontIcon(PlayPause, this.DispatcherQueue);
            UpdateDisplay();
        }

        private void ArtistMixButton_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ArtistMixButton_Loaded;
            this.Unloaded -= ArtistMixButton_Unloaded;
            this.DataContextChanged -= ArtistMixButton_DataContextChanged;
        }

        private void ArtistMixButton_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            UpdateDisplay();
        }

        public MusicX.Core.Models.Button Button
        {
            get => _button;
            set
            {
                _button = value;
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if (_button == null) return;

            // Обновление текстов
            TitleText.Text = _button.Title ?? string.Empty;
            DescriptionText.Text = _button.Description ?? string.Empty;

            // Загрузка изображений
            LoadImages();
        }

        private void LoadImages()
        {
            if (_button?.Images == null || _button.Images.Count == 0)
            {
                // Скрыть GridThumbs, показать placeholder
                fadeOp.Opacity = 0;
                return;
            }

            // Показать GridThumbs
            fadeOp.Opacity = 1;

            // Очистка предыдущих изображений
            GridThumbs.Children.Clear();
            GridThumbsForeground.Children.Clear();

            // Загружаем изображения через вспомогательный метод
            foreach (var item in _button.Images)
            {
                GridThumbs.AddImagesToGrid(item.Url);
            }
            foreach (var item in _button.ForegroundImages)
            {
                GridThumbsForeground.AddImagesToGrid(item.Url);
            }
        }

        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // Анимация появления кнопки воспроизведения
            FadeInAnimationGridPlayIcon.Begin();
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Анимация исчезновения кнопки воспроизведения
            FadeOutAnimationGridPlayIcon.Begin();
        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Запуск микса при нажатии
            PlayMix();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PlayMix();
        }

        private void PlayMix()
        {
            if (_button == null) return;

            // Используем MixId для запуска микса
            string mixId = _button.MixId;
            if (string.IsNullOrEmpty(mixId))
            {
                // Если MixId отсутствует, используем Id или EntityId
                mixId = _button.Id.ToString();
            }


            // 2. Исправляем создание MixOptions
            var mixOptions = new VK_UI3.VKs.IVK.MixOptions(mixId, EntityId: _button.EntityId.ToString());
            new MixAudio(mixOptions, this.DispatcherQueue);
        }
    }
}