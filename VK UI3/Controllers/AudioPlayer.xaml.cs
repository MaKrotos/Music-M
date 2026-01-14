using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.DiscordRPC;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VK_UI3.VKs.IVK;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using VK_UI3.Views.ModalsPages;
using Windows.Foundation;
using VK_UI3.Services;
using MusicX.Services.Player.Sources;
using VK_UI3.Helpers;

namespace VK_UI3.Controllers
{
    /// <summary>
    /// Audio player control for handling music playback
    /// </summary>
    public sealed partial class AudioPlayer : Page, INotifyPropertyChanged
    {
        #region Fields

        private DiscordRichPresenceManager discordRichPresenceManager = new DiscordRichPresenceManager();
        private static WeakEventManager TrackDataThisChanged = new WeakEventManager();

        // Animation fields
        private AnimationsChangeFontIcon changeIconPlayBTN = null;
        private AnimationsChangeFontIcon animateFontIcon = null;
        private AnimationsChangeImage changeImage = null;
        private AnimationsChangeText changeText = null;
        private AnimationsChangeText changeText2 = null;
        private AnimationsChangeFontIcon statusAnimate = null;

        // Player state fields
        private long _trackPositionMs;
        private long _trackDurationMs;
        private long _sliderPositionMs;
        private double _redFillPercent = 0;
        private bool isManualChange = false;
        private double actualHeight = 0;
        private bool enablinUI = false;

        // Marquee animation fields
        private bool isPointerOver = false;
        private bool isTitleAnimating = false;
        private bool isArtistAnimating = false;
        private double titleMarqueeOffset = 0;
        private double artistMarqueeOffset = 0;

        #endregion

        #region Properties

        public MediaPlayer MediaPlayer
        {
            get { return VK_UI3.Services.MediaPlayerService.MediaPlayer; }
            set { VK_UI3.Services.MediaPlayerService.MediaPlayer = value; }
        }

        public static IVKGetAudio iVKGetAudio
        {
            get { return VK_UI3.Services.MediaPlayerService.iVKGetAudio; }
            set
            {
                VK_UI3.Services.MediaPlayerService.iVKGetAudio = value;
                MainView.mainView.setNewPlayingList(value);
                NotifyoniVKUpdate();
            }
        }

        public static AudioEqualizer Equalizer
        {
            get { return VK_UI3.Services.MediaPlayerService.Equalizer; }
            set
            {
                VK_UI3.Services.MediaPlayerService.Equalizer = value;
            }
        }

        public ExtendedAudio TrackDataThis => _TrackDataThisGet().Result;

        public string Thumbnail
        {
            get
            {
                var trackData = TrackDataThis;
                if (trackData?.audio?.Album?.Thumb == null) return "null";

                return trackData.audio.Album.Thumb.Photo600
                     ?? trackData.audio.Album.Thumb.Photo300
                     ?? trackData.audio.Album.Thumb.Photo270
                     ?? trackData.audio.Album.Thumb.Photo68
                     ?? trackData.audio.Album.Thumb.Photo34
                     ?? "null";
            }
        }

        public long TrackPositionMs
        {
            get { return _trackPositionMs; }
            set
            {
                if (_trackPositionMs != value)
                {
                    _trackPositionMs = value;
                    OnPropertyChanged(nameof(TrackPositionMs));
                    UpdateRedFillPercent();
                }
            }
        }

        public long TrackDurationMs
        {
            get { return _trackDurationMs; }
            set
            {
                if (_trackDurationMs != value)
                {
                    _trackDurationMs = value;
                    OnPropertyChanged(nameof(TrackDurationMs));
                    UpdateRedFillPercent();
                }
            }
        }

        public int TrackPosition
        {
            get { return (int)(_trackPositionMs / 1000); }
            set { TrackPositionMs = value * 1000; }
        }

        public long SliderPositionMs
        {
            get => _sliderPositionMs;
            set
            {
                if (_sliderPositionMs != value)
                {
                    _sliderPositionMs = value;
                    OnPropertyChanged(nameof(SliderPositionMs));
                }
            }
        }

        public double RedFillPercent
        {
            get => _redFillPercent;
            set
            {
                if (_redFillPercent != value)
                {
                    _redFillPercent = value;
                    AnimateRedRectangle();
                    OnPropertyChanged(nameof(RedFillPercent));
                }
            }
        }

        public double simpleAudioBind
        {
            get => VK_UI3.Services.MediaPlayerService.MediaPlayer?.Volume * 100 ?? 100;
            set
            {
                var volume = value / 100;
                SettingsTable.SetSetting("Volume", volume.ToString());
                VK_UI3.Services.MediaPlayerService.MediaPlayer.Volume = volume;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructor and Initialization

        public AudioPlayer()
        {
            this.InitializeComponent();
            InitializeEvents();
            InitializeAnimations();
            InitializeSettings();
        }

        private void InitializeEvents()
        {
            this.SizeChanged += RootGrid_SizeChanged;
            if (RootGrid != null)
                RootGrid.SizeChanged += RootGrid_SizeChanged;

            this.Loaded += AudioPlayer_Loaded;
            VK_UI3.Services.MediaPlayerService.oniVKUpdate += AudioPlayer_oniVKUpdate;
            VK_UI3.Services.MediaPlayerService.AudioPlayedChangeEvent += AudioPlayer_AudioPlayedChange;
            VK_UI3.Services.MediaPlayerService.VolumeChanged += MediaPlayerService_VolumeChanged;
            VK_UI3.Services.MediaPlayerService.PositionChanged += MediaPlayerService_PositionChanged;
            VK_UI3.Services.MediaPlayerService.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            TrackDataThisChanged.AddHandler(AudioPlayer_PropertyChanged);
        }

        private void InitializeAnimations()
        {
            changeIconPlayBTN = new AnimationsChangeFontIcon(this.PlayBTN, this.DispatcherQueue);
            animateFontIcon = new AnimationsChangeFontIcon(this.repeatBTNIcon, this.DispatcherQueue);
            changeImage = new AnimationsChangeImage(this.ImageThumb, DispatcherQueue);
            changeText = new AnimationsChangeText(ArtistTextBlock, this.DispatcherQueue);
            changeText2 = new AnimationsChangeText(TitleTextBlock, this.DispatcherQueue);
            statusAnimate = new AnimationsChangeFontIcon(StatusBTNIcon, this.DispatcherQueue);
        }

        private void InitializeSettings()
        {
            var volumeSetting = SettingsTable.GetSetting("Volume");
            if (volumeSetting != null)
            {
                VK_UI3.Services.MediaPlayerService.MediaPlayer.Volume = double.Parse(volumeSetting.settingValue);
            }

            var enabledSetting = SettingsTable.GetSetting("Equalizer_Enabled", "1");
            if (enabledSetting.settingValue == "1")
            {
                var eqControl = new EqualizerControl();
                eqControl.LoadSettings();
                Equalizer = eqControl.getEqualizes();
            }

            setStatusIcon();
        }

        #endregion

        #region Event Handlers

        #region UI Events

        private void AudioPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            actualHeight = pageRa.ActualHeight;
            if (iVKGetAudio == null)
            {
                DisableAllChildren(this);
            }
        }

        private void AudioPlayer_Unloaded(object sender, RoutedEventArgs e)
        {
            VK_UI3.Services.MediaPlayerService.AudioPlayedChangeEvent -= AudioPlayer_AudioPlayedChange;
            VK_UI3.Services.MediaPlayerService.VolumeChanged -= MediaPlayerService_VolumeChanged;
            VK_UI3.Services.MediaPlayerService.PositionChanged -= MediaPlayerService_PositionChanged;
            VK_UI3.Services.MediaPlayerService.MediaPlayer.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
        }

        private void AudioPlayer_oniVKUpdate(object sender, EventArgs e)
        {
            DisableAllChildren(this, true);
            setButtonPlayNext();
        }

        private void AudioPlayer_AudioPlayedChange(object sender, EventArgs e)
        {
            UpdateTrackInfoDisplay();
            UpdateDiscordState();
        }

        private void MediaPlayerService_VolumeChanged(object sender, VK_UI3.Services.Player.VolumeChangedEventArgs e)
        {
            OnPropertyChanged(nameof(simpleAudioBind));
        }

        private void MediaPlayerService_PositionChanged(object sender, TimeSpan e)
        {
            TrackPositionMs = (long)e.TotalMilliseconds;
            SliderPositionMs = TrackPositionMs;
        }

        private void AudioPlayer_PropertyChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(TrackDataThis));
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AnimateRedRectangle();
            this.DispatcherQueue.TryEnqueue(() =>
            {
                sliderTrackGridUP.Width = RootGrid.ActualWidth;
            });
        }

        #endregion

        #region Button Click Handlers

        private void Button_Play_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TogglePlayPause();
        }

        private void PreviousBTN_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VK_UI3.Services.MediaPlayerService.HandlePreviousTrack();
        }

        private void NextBTN_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PlayNextTrack();
        }

        private void goToPlayList_BTN(object sender, TappedRoutedEventArgs e)
        {
            MainView.mainView.TogglePlayNowPanel();
        }

        private void repeatBTN_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CycleRepeatMode();
        }

        private void TranslatetoStatus_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ToggleShareStatus();
        }

        private void trackDoingBTN_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (VK_UI3.Services.MediaPlayerService.PlayingTrack == null) return;
            FlyOutControl.ShowAt(sender as Button);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VK_UI3.Services.MediaPlayerService.NotifyonClickonTrack();
        }

        private void openLirycBTN_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MainView.mainView.ToggleLyricsPanel();
        }

        private void OpenEqalizer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Open equalizer dialog
        }

        #endregion

        #region Slider Events

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (isManualChange)
            {
                SliderPositionMs = (long)e.NewValue;
                VK_UI3.Services.MediaPlayerService.MediaPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(e.NewValue);
            }
        }

        private void VolumeSlider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            isManualChange = false;
        }

        private void VolumeSlider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            isManualChange = true;
        }

        private void VolumeSlider_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            isManualChange = false;
            SliderPositionMs = TrackPositionMs;
        }

        private void SoundSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Handle sound slider value changes if needed
        }

        private void SoundSlider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // Handle sound slider pointer entered event if needed
        }

        private void SoundSlider_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Handle sound slider pointer exited event if needed
        }

        #endregion

        #region Marquee Animation Events

        private void TitleTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            TitleTranslate.X = 0;
        }

        private void ArtistTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            ArtistTranslate.X = 0;
        }

        private async void Page_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            isPointerOver = true;
            var storyboard = (Storyboard)this.Resources["SliderTrackMoveUp"];
            storyboard.Begin();
            _ = StartTitleMarqueeIfNeeded();
            _ = StartArtistMarqueeIfNeeded();
        }

        private void Page_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            isPointerOver = false;
            var storyboard = (Storyboard)this.Resources["SliderTrackMoveDown"];
            storyboard.Begin();
            ReverseTitleMarquee();
            ReverseArtistMarquee();
        }

        private void TitleClipGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TitleClipGrid.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, TitleClipGrid.ActualWidth, TitleClipGrid.ActualHeight)
            };
        }

        private void ArtistClipGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ArtistClipGrid.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, ArtistClipGrid.ActualWidth, ArtistClipGrid.ActualHeight)
            };
        }

        #endregion

        #endregion

        #region Public Methods

        public static void NotifyoniVKUpdate()
        {
            VK_UI3.Services.MediaPlayerService.NotifyoniVKUpdate();
        }

        public static void NotifyonClickonTrack()
        {
            VK_UI3.Services.MediaPlayerService.NotifyonClickonTrack();
        }

        internal static void PlayNextTrack()
        {
            VK_UI3.Services.MediaPlayerService.PlayNextTrack();
        }


        internal static async void PlayList(IVKGetAudio userAudio)
        {
            VK_UI3.Services.MediaPlayerService.PlayList(userAudio);
        }

        #endregion

        #region Private Methods

        #region UI Helper Methods

        private void TogglePlayPause()
        {
            switch (VK_UI3.Services.MediaPlayerService.MediaPlayer.CurrentState)
            {
                case MediaPlayerState.Playing:
                    VK_UI3.Services.MediaPlayerService.MediaPlayer.Pause();
                    break;
                case MediaPlayerState.Paused:
                    VK_UI3.Services.MediaPlayerService.MediaPlayer.Play();
                    break;
            }
        }

        private void UpdateTrackInfoDisplay()
        {
            TrackDurationMs = (long)(TrackDataThis?.audio?.Duration * 1000 ?? 0);
            OnPropertyChanged(nameof(TrackPosition));
            OnPropertyChanged(nameof(TrackDataThis));

            changeImage.ChangeImageWithAnimation(Thumbnail);
            changeText.ChangeTextWithAnimation(TrackDataThis?.audio?.Artist ?? string.Empty);
            changeText2.ChangeTextWithAnimation(TrackDataThis?.audio?.Title ?? string.Empty);

            FlyOutControl.dataTrack = TrackDataThis;
        }

        private void HandleTrackPlayedHighlighting()
        {
            var highlightSetting = SettingsTable.GetSetting("HighlightPlayedTracks");
            if (highlightSetting != null && highlightSetting.settingValue.Equals("1") && VK_UI3.Services.MediaPlayerService.PlayingTrack != null)
            {
                VK_UI3.Services.MediaPlayerService.PlayingTrack.iVKGetAudio.SelectAudio(VK_UI3.Services.MediaPlayerService.PlayingTrack);
            }
        }

        private void CycleRepeatMode()
        {
            try
            {
                var currentSetting = SettingsTable.GetSetting("playNext").settingValue;
                string newSetting = currentSetting switch
                {
                    "RepeatOne" => "RepeatAll",
                    "RepeatAll" => "Shuffle",
                    "Shuffle" => "RepeatOne",
                    _ => "RepeatOne"
                };

                SettingsTable.SetSetting("playNext", newSetting);
                setButtonPlayNext();
            }
            catch { }
        }

        private void ToggleShareStatus()
        {
            var share = SettingsTable.GetSetting("shareFriend");
            if (share.settingValue == "false")
            {
                SettingsTable.SetSetting("shareFriend", "true");
                iVKGetAudio?.shareToVK();
            }
            else
            {
                SettingsTable.SetSetting("shareFriend", "false");
            }
            setStatusIcon();
        }

        private void setButtonPlayNext()
        {
            try
            {
                if (SettingsTable.GetSetting("playNext") == null)
                {
                    SettingsTable.SetSetting("playNext", "RepeatAll");
                }

                switch (SettingsTable.GetSetting("playNext").settingValue)
                {
                    case "RepeatOne":
                        animateFontIcon.ChangeFontIconWithAnimation("\uE8ED");
                        iVKGetAudio?.UnShuffleList();
                        break;
                    case "Shuffle":
                        animateFontIcon.ChangeFontIconWithAnimation("\uE8B1");
                        iVKGetAudio?.setShuffle();
                        break;
                    case "RepeatAll":
                        animateFontIcon.ChangeFontIconWithAnimation("\uE8EE");
                        iVKGetAudio?.UnShuffleList();
                        break;
                }
            }
            catch { }
        }

        public void setStatusIcon()
        {
            var share = SettingsTable.GetSetting("shareFriend");
            if (share == null)
            {
                share = new SettingsTable
                {
                    settingName = "shareFriend",
                    settingValue = "false"
                };
                DatabaseHandler.getConnect().Insert(share);
            }

            if (bool.Parse(share.settingValue))
            {
                statusAnimate.ChangeFontIconWithAnimation("\uE701");
            }
            else
            {
                statusAnimate.ChangeFontIconWithAnimation("\uEB5E");
            }
        }

        private void UpdateDiscordState()
        {
            var setting = DB.SettingsTable.GetSetting("DisableDiscordIntegration");
            if (setting != null && setting.settingValue.Equals("1"))
                return;

            discordRichPresenceManager.SetTrack(TrackDataThis, VK_UI3.Services.MediaPlayerService.MediaPlayer);
        }

        private void DisableAllChildren(DependencyObject parent, bool enable = false)
        {
            if (enable == enablinUI) return;
            enablinUI = enable;

            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is Control control)
                    {
                        control.IsEnabled = enable;
                    }
                    DisableAllChildren(child, enable);
                }
            });
        }

        #endregion

        #region Animation Methods

        private void AnimateRedRectangle()
        {
            if (RootGrid == null || RedRectangle == null) return;

            this.DispatcherQueue.TryEnqueue(() =>
            {
                double toWidth = RootGrid.ActualWidth * RedFillPercent;
                var animation = new DoubleAnimation
                {
                    To = toWidth,
                    Duration = new Duration(TimeSpan.FromMilliseconds(40)),
                    EnableDependentAnimation = true,
                };

                Storyboard.SetTarget(animation, RedRectangle);
                Storyboard.SetTargetProperty(animation, "Width");
                var sb = new Storyboard();
                sb.Children.Add(animation);
                sb.Begin();
                RedRectangle.Width = toWidth;
            });
        }

        private void UpdateRedFillPercent()
        {
            if (TrackDurationMs > 0)
                SetRedFillPercent((double)TrackPositionMs / TrackDurationMs);
            else
                SetRedFillPercent(0);
        }

        private void SetRedFillPercent(double percent)
        {
            percent = Math.Clamp(percent, 0, 1);
            RedFillPercent = percent;
        }

        private async Task AnimateTranslate(TranslateTransform transform, double to, double durationSeconds)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                EnableDependentAnimation = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(animation, transform);
            Storyboard.SetTargetProperty(animation, "X");
            storyboard.Children.Add(animation);
            storyboard.Begin();
            await Task.Delay((int)(durationSeconds * 1000));
        }

        private async Task StartTitleMarqueeIfNeeded()
        {
            if (isTitleAnimating) return;
            isTitleAnimating = true;

            await Task.Delay(100);
            var outerGrid = (FrameworkElement)TitleClipGrid.Parent;
            double containerWidth = outerGrid.ActualWidth;
            double textWidth = TitleTextBlock.ActualWidth;

            if (textWidth > containerWidth && isPointerOver)
            {
                titleMarqueeOffset = containerWidth - textWidth - 15;
                while (isPointerOver)
                {
                    await AnimateTranslate(TitleTranslate, titleMarqueeOffset, 3);
                    await AnimateTranslate(TitleTranslate, 0, 1);
                    await Task.Delay(2000);
                }
            }
            isTitleAnimating = false;
        }

        private async Task StartArtistMarqueeIfNeeded()
        {
            if (isArtistAnimating) return;
            isArtistAnimating = true;

            await Task.Delay(100);
            var outerGrid = (FrameworkElement)ArtistClipGrid.Parent;
            double containerWidth = outerGrid.ActualWidth;
            double textWidth = ArtistTextBlock.ActualWidth;

            if (textWidth > containerWidth && isPointerOver)
            {
                artistMarqueeOffset = containerWidth - textWidth - 15;
                while (isPointerOver)
                {
                    await AnimateTranslate(ArtistTranslate, artistMarqueeOffset, 3);
                    await AnimateTranslate(ArtistTranslate, 0, 1);
                    await Task.Delay(2000);
                }
            }
            isArtistAnimating = false;
        }

        private void ReverseTitleMarquee()
        {
            _ = AnimateTranslate(TitleTranslate, 0, 1);
        }

        private void ReverseArtistMarquee()
        {
            _ = AnimateTranslate(ArtistTranslate, 0, 1);
        }

        #endregion

        #region Data Access Methods

        public static async Task<ExtendedAudio> _TrackDataThisGet(bool forced = false)
        {
            if (iVKGetAudio != null && iVKGetAudio.countTracks != 0)
            {
                return await iVKGetAudio.GetTrackPlay(forced);
            }
            return VK_UI3.Services.MediaPlayerService._trackDataThis;
        }

        #endregion

        #endregion

        #region Property Change Notification

        protected void OnPropertyChanged(string propertyName)
        {
            try
            {
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                });
            }
            catch { }
        }

        #endregion

        #region MediaPlayer Event Handlers

        private void MediaPlayer_CurrentStateChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                switch (sender.CurrentState)
                {
                    case MediaPlayerState.Playing:
                        changeIconPlayBTN.ChangeFontIconWithAnimation("\uE769"); // Pause icon
                        break;
                    case MediaPlayerState.Paused:
                        changeIconPlayBTN.ChangeFontIconWithAnimation("\uE768"); // Play icon
                        break;
                    case MediaPlayerState.Closed:
                        changeIconPlayBTN.ChangeFontIconWithAnimation("\uE768"); // Play icon
                        break;
                }
            });
        }

        #endregion
    }
}