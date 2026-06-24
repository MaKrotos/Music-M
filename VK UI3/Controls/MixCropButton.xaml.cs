using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MusicX.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using VK_UI3.Helpers.Animations;
using VK_UI3.VKs.IVK;

namespace VK_UI3.Controls
{
    public sealed partial class MixCropButton : UserControl
    {
        public MixCropButton()
        {
            this.InitializeComponent();

            _animationsChangeFontIcon = new AnimationsChangeFontIcon(PlayPause, this.DispatcherQueue);
            _titleAnim = new AnimationsChangeText(Title, this.DispatcherQueue);

            this.Unloaded += MixCropButton_Unloaded;
            this.Loaded += MixCropButton_Loaded;
            DataContextChanged += MixCropButton_DataContextChanged;
        }

        private void MixCropButton_Loaded(object sender, RoutedEventArgs e)
        {
            FadeOutAnimationGrid.Completed += FadeOutAnimationGrid_Completed;
            Services.MediaPlayerService.oniVKUpdate += AudioPlayer_oniVKUpdate;
        }

        private void MixCropButton_Unloaded(object sender, RoutedEventArgs e)
        {
            Services.MediaPlayerService.oniVKUpdate -= AudioPlayer_oniVKUpdate;
            this.Unloaded -= MixCropButton_Unloaded;
            this.Loaded -= MixCropButton_Loaded;
            FadeOutAnimationGrid.Completed -= FadeOutAnimationGrid_Completed;
            DataContextChanged -= MixCropButton_DataContextChanged;
        }

        private void AudioPlayer_oniVKUpdate(object sender, EventArgs e)
        {
            UpdatePlayState();
        }

        private void UpdatePlayState(bool prin = false, bool pause = false)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                string icon = "\uE768"; // default icon

                if (prin)
                {
                    icon = pause ? "\uE768" : "\uE769";
                }
                else if (IsThisMixNowPlaying && VK_UI3.Services.MediaPlayerService.MediaPlayer.PlaybackSession.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Paused)
                {
                    icon = "\uE769";
                }

                if (!IsThisMixNowPlaying)
                {
                    if (!_entered)
                    {
                        FadeInAnimationGridPlayIcon.Pause();
                        FadeOutAnimationGridPlayIcon.Begin();
                    }
                }
                else
                {
                    FadeOutAnimationGridPlayIcon.Pause();
                    FadeInAnimationGridPlayIcon.Begin();
                }

                _animationsChangeFontIcon.ChangeFontIconWithAnimation(icon);
            });
        }

        public bool IsThisMixNowPlaying
        {
            get
            {
                if (Button == null || VK_UI3.Services.MediaPlayerService.iVKGetAudio == null)
                    return false;

                if (!(VK_UI3.Services.MediaPlayerService.iVKGetAudio is MixAudio))
                    return false;

                var mix = (VK_UI3.Services.MediaPlayerService.iVKGetAudio as MixAudio);

                // Сравниваем MixId или другие идентификаторы
                // if (mix.MixId == _button.MixId || mix.EntityId == _button.EntityId)
                //     return true;

                return false;
            }
        }

        private AnimationsChangeText _titleAnim { get; set; }

        private AnimationsChangeFontIcon _animationsChangeFontIcon { get; set; }
        public MusicX.Core.Models.Button Button { get; set; }


        private bool _entered;

        private void MixCropButton_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            Button = (DataContext as MusicX.Core.Models.Button);
            UpdateDisplay();
        }

        private void FadeOutAnimationGrid_Completed(object sender, object e)
        {
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (Button == null) return;

            _animationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
            _titleAnim.ChangeTextWithAnimation(Button.Title ?? string.Empty);

            // Загрузка изображений
            if (Button.Images != null && Button.Images.Any())
            {
                // Очищаем предыдущие изображения
                GridThumbs.Children.Clear();

                // Добавляем изображения в GridImagesCustom
                foreach (var image in Button.Images)
                {
                    GridThumbs.AddImagesToGrid(image.Url);
                }

                // Загрузка изображений
                if (Button.Images != null && Button.Images.Any())
                {
                    // Очищаем предыдущие изображения
                    GridThumbs.Children.Clear();

                    // Добавляем все изображения в GridImagesCustom
                    var imageUrls = Button.Images.Select(img => img.Url).ToList();
                    GridThumbs.AddImagesToGrid(imageUrls);

                }
            }

            UpdatePlayState();
            FadeInAnimationGrid.Begin();
        }

        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _entered = true;
            FadeOutAnimationGridPlayIcon.Pause();
            FadeInAnimationGridPlayIcon.Begin();
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _entered = false;
            if (!IsThisMixNowPlaying)
            {
                FadeInAnimationGridPlayIcon.Pause();
                FadeOutAnimationGridPlayIcon.Begin();
            }
        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(sender as UIElement).Properties.IsLeftButtonPressed)
            {
                PlayMix();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PlayMix();
        }

        private void PlayMix()
        {
            if (Button == null) return;

            if (IsThisMixNowPlaying)
            {
                if (VK_UI3.Services.MediaPlayerService.MediaPlayer.PlaybackSession.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Paused)
                {
                    VK_UI3.Services.MediaPlayerService.MediaPlayer.Pause();
                    UpdatePlayState(true, true);
                }
                else
                {
                    VK_UI3.Services.MediaPlayerService.MediaPlayer.Play();
                    UpdatePlayState(true, false);
                }
            }
            else
            {
                _animationsChangeFontIcon.ChangeFontIconWithAnimation("\uE916");
                Task.Run(async () =>
                {

                    string mixId = Button.MixId;
                    string entityId = null;
                    ImmutableDictionary<string, ImmutableArray<string>> options = null;



                    // Это обычный микс с опциями (жанр/настроение)
                    var mixOptionsData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(Button.MixOptions);

                    if (mixOptionsData != null)
                    {
                        var optionsBuilder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<string>>();

                        foreach (var kvp in mixOptionsData)
                        {
                            optionsBuilder.Add(kvp.Key, kvp.Value.ToImmutableArray());
                        }

                        options = optionsBuilder.ToImmutable();
                    }

                    var mixOptions = new MixOptions(
                        Id: mixId,
                        Append: 0,
                        Options: options,
                        PromptEvents: null,
                        Ref: null,
                        EntityId: null
                    );

                    new MixAudio(mixOptions, this.DispatcherQueue);
                });
            }
        }
    }
}