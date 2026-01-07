using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using MusicX.Services.Player;
using MusicX.Services.Player.Sources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.DiscordRPC;
using VK_UI3.DownloadTrack;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
         using VK_UI3.Views.ModalsPages;
         using Windows.Foundation;
         using VK_UI3.Services;

namespace VK_UI3.Controllers
{
    /// <summary>
    /// Audio player control for handling music playback
    /// </summary>
    public sealed partial class AudioPlayer : Microsoft.UI.Xaml.Controls.Page, INotifyPropertyChanged
    {
        #region Constants and Static Fields

        private const int GWL_WNDPROC = -4;
                 private const int WM_APPCOMMAND = 0x0319;

        #endregion

        #region Events
        
                #endregion

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
        private Storyboard storyboard = new Storyboard();
        private Symbol? symbolNow = null;

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

        // Win32 hook fields
        private nint _oldWndProc = 0;
        private delegate nint WndProcDelegate(nint hWnd, int msg, nint wParam, nint lParam);
        private WndProcDelegate _wndProcDelegate;
        
        // Media key handling prevention

        #endregion

        #region Properties
        
        // Media key handling prevention

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
                         // Note: PlayTrack(position: mediaPlayer.Position); should be handled elsewhere
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
            InitializeMediaPlayer();
            SetupSystemMediaTransportControls();
            // Отключаем Win32 хук, чтобы избежать дублирования
            // InitializeWin32Hook();
        }

        private void InitializeEvents()
        {
            this.SizeChanged += RootGrid_SizeChanged;
            if (RootGrid != null)
                RootGrid.SizeChanged += RootGrid_SizeChanged;

            this.Loaded += AudioPlayer_Loaded;
            VK_UI3.Services.MediaPlayerService.oniVKUpdate += AudioPlayer_oniVKUpdate;
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

        private void SetupSystemMediaTransportControls()
        {
            try
                         {
                             var systemControls = VK_UI3.Services.MediaPlayerService.MediaPlayer.SystemMediaTransportControls;
            
                             // Включить кнопки
                             systemControls.IsPlayEnabled = true;
                             systemControls.IsPauseEnabled = true;
                             systemControls.IsNextEnabled = true;
                             systemControls.IsPreviousEnabled = true;
                             systemControls.IsStopEnabled = true;
            
                             // Подписаться на события
                             systemControls.ButtonPressed += SystemControls_ButtonPressed;
            
                             // Настроить отображение
                             systemControls.DisplayUpdater.Type = MediaPlaybackType.Music;
                             systemControls.DisplayUpdater.AppMediaId = "MusicM";
            
                             UpdateSystemMediaDisplay();
                         }
                         catch (Exception ex)
                         {
                             Console.WriteLine($"Error setting up system media controls: {ex.Message}");
                         }
        }

        private void SystemControls_ButtonPressed(SystemMediaTransportControls sender,
            SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                // Debounce для предотвращения двойного срабатывания
                var now = DateTime.Now;
                if ((now - VK_UI3.Services.MediaPlayerService._lastMediaKeyTime).TotalMilliseconds < VK_UI3.Services.MediaPlayerService.MEDIA_KEY_DEBOUNCE_MS)
                    return;
                
                VK_UI3.Services.MediaPlayerService._lastMediaKeyTime = now;
                
                if (VK_UI3.Services.MediaPlayerService._isProcessingMediaKey)
                    return;
                
                VK_UI3.Services.MediaPlayerService._isProcessingMediaKey = true;
                
                try
                {
                    switch (args.Button)
                    {
                        case SystemMediaTransportControlsButton.Play:
                                                     VK_UI3.Services.MediaPlayerService.MediaPlayer.Play();
                                                     break;
                                                 case SystemMediaTransportControlsButton.Pause:
                                                     VK_UI3.Services.MediaPlayerService.MediaPlayer.Pause();
                                                     break;
                                                 case SystemMediaTransportControlsButton.Next:
                                                     VK_UI3.Services.MediaPlayerService.PlayNextTrack();
                                                     break;
                                                 case SystemMediaTransportControlsButton.Previous:
                                                     if (VK_UI3.Services.MediaPlayerService.MediaPlayer.Position.TotalSeconds >= 3)
                                                         VK_UI3.Services.MediaPlayerService.MediaPlayer.Position = TimeSpan.Zero;
                                                     else
                                                         VK_UI3.Services.MediaPlayerService.PlayPreviousTrack();
                                                     break;
                                                 case SystemMediaTransportControlsButton.Stop:
                                                     VK_UI3.Services.MediaPlayerService.MediaPlayer.Pause();
                                                     VK_UI3.Services.MediaPlayerService.MediaPlayer.Position = TimeSpan.Zero;
                                                     break;
                    }
                }
                finally
                {
                    Task.Delay(VK_UI3.Services.MediaPlayerService.MEDIA_KEY_DEBOUNCE_MS).ContinueWith(_ =>
                    {
                        VK_UI3.Services.MediaPlayerService._isProcessingMediaKey = false;
                    });
                }
            });
        }

        private void UpdateSystemMediaDisplay()
        {
            if (TrackDataThis == null || TrackDataThis.audio == null) return;

            try
            {
                var systemControls = VK_UI3.Services.MediaPlayerService.MediaPlayer.SystemMediaTransportControls;
                                 var updater = systemControls.DisplayUpdater;

                updater.ClearAll();
                updater.Type = MediaPlaybackType.Music;

                var audio = TrackDataThis.audio;

                // Основная информация о треке
                updater.MusicProperties.Title = audio.Title ?? "Unknown Title";
                updater.MusicProperties.Artist = audio.Artist ?? "Unknown Artist";

                // Информация об альбоме
                if (audio.Album != null)
                {
                    updater.MusicProperties.AlbumTitle = audio.Album.Title ?? string.Empty;
                }

                // Обложка трека
                if (audio.Album?.Thumb != null)
                {
                    var thumbnailUrl = audio.Album.Thumb.Photo600 ??
                                     audio.Album.Thumb.Photo300 ??
                                     audio.Album.Thumb.Photo270;

                    if (thumbnailUrl != null)
                    {
                        try
                        {
                            RandomAccessStreamReference imageStreamRef =
                                RandomAccessStreamReference.CreateFromUri(new Uri(thumbnailUrl));
                            updater.Thumbnail = imageStreamRef;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting thumbnail: {ex.Message}");
                        }
                    }
                }

                updater.Update();

                // Обновить статус воспроизведения
                systemControls.PlaybackStatus = VK_UI3.Services.MediaPlayerService.MediaPlayer.PlaybackSession.PlaybackState switch
                                 {
                                     MediaPlaybackState.Playing => MediaPlaybackStatus.Playing,
                                     MediaPlaybackState.Paused => MediaPlaybackStatus.Paused,
                                     MediaPlaybackState.Buffering => MediaPlaybackStatus.Changing,
                                     _ => MediaPlaybackStatus.Stopped
                                 };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating system media display: {ex.Message}");
            }
        }

        private void InitializeMediaPlayer()
        {
            TrackDurationMs = 0;
            TrackPosition = 0;

            VK_UI3.Services.MediaPlayerService.MediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
            
                         // Отключаем CommandManager, чтобы избежать дублирования с SystemMediaTransportControls
                         VK_UI3.Services.MediaPlayerService.MediaPlayer.CommandManager.IsEnabled = false;
            
                         VK_UI3.Services.MediaPlayerService.MediaPlayer.SystemMediaTransportControls.DisplayUpdater.Type = MediaPlaybackType.Music;
            
                         // Media player events
                         VK_UI3.Services.MediaPlayerService.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
                         VK_UI3.Services.MediaPlayerService.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
                         VK_UI3.Services.MediaPlayerService.MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
                         VK_UI3.Services.MediaPlayerService.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                         VK_UI3.Services.MediaPlayerService.MediaPlayer.SourceChanged += MediaPlayer_SourceChanged;
            
                         // Playback session events
                         VK_UI3.Services.MediaPlayerService.MediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
                         VK_UI3.Services.MediaPlayerService.MediaPlayer.PlaybackSession.BufferedRangesChanged += PlaybackSession_BufferedRangesChanged;
                         VK_UI3.Services.MediaPlayerService.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
        }

        #endregion

        #region Event Handlers

        #region Player Events

        private void MediaPlayer_MediaOpened(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            TrackDurationMs = (long)(TrackDataThis?.audio?.Duration * 1000 ?? 0);
        }

        private void MediaPlayer_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            HandleTrackPlayedHighlighting();

            if (SettingsTable.GetSetting("playNext").settingValue.Equals("RepeatOne"))
                             // Note: PlayTrack() should be handled by the service
                             VK_UI3.Services.MediaPlayerService.PlayNextTrack();
                         else
                             VK_UI3.Services.MediaPlayerService.PlayNextTrack();
        }

        private void MediaPlayer_MediaFailed(Windows.Media.Playback.MediaPlayer sender,
            Windows.Media.Playback.MediaPlayerFailedEventArgs args)
        {
            Console.WriteLine($"Media failed: {args.Error}, {args.ErrorMessage}");
        }

        private void MediaPlayer_CurrentStateChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                switch (sender.CurrentState)
                                 {
                                     case MediaPlayerState.Playing:
                                         changeIconPlayBTN.ChangeFontIconWithAnimation("\uE769");
                                         UpdateSystemMediaDisplay();
                                         break;
                                     case MediaPlayerState.Paused:
                                         changeIconPlayBTN.ChangeFontIconWithAnimation("\uE768");
                                         UpdateSystemMediaDisplay();
                                         break;
                                     case MediaPlayerState.Closed:
                                         // Note: PlayTrack(iVKGetAudio?.currentTrack); should be handled by the service
                                         break;
                                 }
            });
        }

        private void MediaPlayer_SourceChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            if (TrackDataThis == null) return;

            UpdateTrackInfoDisplay();
            UpdateSystemMediaDisplay();

            if (bool.Parse(SettingsTable.GetSetting("shareFriend").settingValue))
                iVKGetAudio?.shareToVK();

            UpdateDiscordState();
        }

        private void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            if (!isManualChange)
            {
                TrackPositionMs = (long)sender.Position.TotalMilliseconds;
                SliderPositionMs = TrackPositionMs;
            }
            else
            {
                TrackPositionMs = (long)sender.Position.TotalMilliseconds;
            }
        }

        private void PlaybackSession_BufferedRangesChanged(MediaPlaybackSession sender, object args)
        {
            // Handle buffering updates if needed
        }

        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                UpdateSystemMediaDisplay();
            });
        }

        #endregion

        #region UI Events

        private void AudioPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            actualHeight = pageRa.ActualHeight;
            if (iVKGetAudio == null)
            {
                DisableAllChildren(this);
            }
        }

        private void AudioPlayer_oniVKUpdate(object sender, EventArgs e)
        {
            DisableAllChildren(this, true);
            setButtonPlayNext();
        }

        private void AudioPlayer_PropertyChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(TrackDataThis));
        }

        private void AudioPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Property change handling
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

        private void PlayButton_Click(object sender, RoutedEventArgs e) { }
        private void PauseButton_Click(object sender, RoutedEventArgs e) { }
        private void StopButton_Click(object sender, RoutedEventArgs e) { }

        private void Button_Play_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TogglePlayPause();
        }

        private void PreviousBTN_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PlayPreviousTrack();
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

        private void SoundSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) { }
        private void SoundSlider_PointerEntered(object sender, PointerRoutedEventArgs e) { }
        private void SoundSlider_PointerExited(object sender, PointerRoutedEventArgs e) { }

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

        internal static void PlayPreviousTrack()
                 {
                     VK_UI3.Services.MediaPlayerService.PlayPreviousTrack();
                 }

        internal static async void PlayList(IVKGetAudio userAudio)
                 {
                     VK_UI3.Services.MediaPlayerService.PlayList(userAudio);
                 }

        #endregion

        #region Private Methods

        #region Player Control Methods

        private async static Task PlayTrack(long? v = 0, TimeSpan? position = null)
        {
            if (iVKGetAudio == null)
                return;

            VK_UI3.Services.MediaPlayerService._tokenSource?.Cancel();
            VK_UI3.Services.MediaPlayerService._tokenSource?.Dispose();
            VK_UI3.Services.MediaPlayerService._tokenSource = new();

            await EnsureTrackListLoaded();

            if (iVKGetAudio.listAudio.Count == 0) return;

            if (v != null && iVKGetAudio.currentTrack == null)
                iVKGetAudio.currentTrack = (long)v;

            var trackdata = await _TrackDataThisGet(true);
            if (trackdata == null)
                return;

            SendPlaybackStatistics(trackdata);
            SendVKStartEvent(trackdata);

            if (ShouldSkipTrack(trackdata))
            {
                PlayNextTrack();
                return;
            }

            await LoadAndPlayTrack(trackdata, position);
        }

        private static async Task EnsureTrackListLoaded()
        {
            if (iVKGetAudio.listAudio.Count == 0)
            {
                var tcs = new TaskCompletionSource<bool>();
                EventHandler handler = null;
                handler = (sender, args) =>
                {
                    iVKGetAudio.onListUpdate -= handler;
                    tcs.SetResult(true);
                };
                iVKGetAudio.onListUpdate += handler;

                iVKGetAudio.GetTracks();
                await tcs.Task;
            }
        }

        private static void SendPlaybackStatistics(ExtendedAudio trackdata)
        {
            ExtendedAudio previousTrack = VK_UI3.Services.MediaPlayerService.PlayingTrack;
            int? previousTrackPlayedSeconds = null;

            if (previousTrack != null)
            {
                previousTrackPlayedSeconds = (int)VK_UI3.Services.MediaPlayerService.MediaPlayer.Position.TotalSeconds;
            }

            _ = KrotosVK.sendVKAudioPlayStat(
                trackdata,
                previousTrack,
                previousTrackPlayedSeconds
            );
        }

        private static void SendVKStartEvent(ExtendedAudio trackdata)
        {
            if (iVKGetAudio is PlayListVK playlistVK)
            {
                VK.sendStartEvent((long)trackdata.audio.Id,
                    (long)trackdata.audio.OwnerId,
                    playlistVK.playlist.Id);
            }
            else
            {
                VK.sendStartEvent((long)trackdata.audio.Id,
                    (long)trackdata.audio.OwnerId);
            }
        }

        private static bool ShouldSkipTrack(ExtendedAudio trackdata)
        {
            return trackdata.audio.Url == null ||
                   new DB.SkipPerformerDB().skipIsSet(trackdata.audio.Artist);
        }

        private static async Task LoadAndPlayTrack(ExtendedAudio trackdata, TimeSpan? position)
        {
            var mediaSource = Windows.Media.Core.MediaSource.CreateFromUri(new Uri(trackdata.audio.Url.ToString()));
            var mediaPlaybackItem = new Windows.Media.Playback.MediaPlaybackItem(mediaSource);

            VK_UI3.Services.MediaPlayerService.PlayingTrack = trackdata;
            VK_UI3.Services.MediaPlayerService.MediaPlayer.Pause();

            if (new CheckFFmpeg().IsExist())
            {
                await LoadWithMediaSources(trackdata);
            }
            else
            {
                LoadBasicMediaItem(trackdata, mediaPlaybackItem);
            }

            if (position != null)
            {
                VK_UI3.Services.MediaPlayerService.MediaPlayer.Position = (TimeSpan)position;
            }

            VK_UI3.Services.MediaPlayerService.MediaPlayer.Play();
            iVKGetAudio.ChangePlayAudio(trackdata);
            VK_UI3.Services.MediaPlayerService.NotifyAudioPlayedChange(trackdata);
        }

        private static async Task LoadWithMediaSources(ExtendedAudio trackdata)
        {
            var allSourcesTask = Task.WhenAll(VK_UI3.Services.MediaPlayerService._mediaSources.Select(b =>
                             b.OpenWithMediaPlayerAsync(VK_UI3.Services.MediaPlayerService.MediaPlayer, trackdata.audio, VK_UI3.Services.MediaPlayerService._tokenSource.Token, equalizer: VK_UI3.Services.MediaPlayerService.Equalizer)));

            try
            {
                await allSourcesTask;
            }
            catch
            {
                if (allSourcesTask.IsCanceled ||
                    allSourcesTask.Exception?.InnerExceptions.All(b => b is OperationCanceledException) is true)
                    return;

                throw;
            }

            if (!allSourcesTask.Result.Any(b => b))
            {
                PlayNextTrack();
                return;
            }
        }

        private static void LoadBasicMediaItem(ExtendedAudio trackdata, MediaPlaybackItem mediaPlaybackItem)
        {
            MediaItemDisplayProperties props = mediaPlaybackItem.GetDisplayProperties();
            props.Type = Windows.Media.MediaPlaybackType.Music;
            props.MusicProperties.Title = trackdata.audio.Title;
            props.MusicProperties.AlbumArtist = trackdata.audio.Artist;

            if (trackdata.audio.Album?.Thumb != null)
            {
                var thumbnailUrl = trackdata.audio.Album.Thumb.Photo600 ??
                                 trackdata.audio.Album.Thumb.Photo270 ??
                                 trackdata.audio.Album.Thumb.Photo300;

                if (thumbnailUrl != null)
                {
                    RandomAccessStreamReference imageStreamRef =
                        RandomAccessStreamReference.CreateFromUri(new Uri(thumbnailUrl));
                    props.Thumbnail = imageStreamRef;
                }
            }

            mediaPlaybackItem.ApplyDisplayProperties(props);
            VK_UI3.Services.MediaPlayerService.MediaPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(1);
                         MainWindow.mainWindow.requstDownloadFFMpegAsync();
                         VK_UI3.Services.MediaPlayerService.MediaPlayer.Source = mediaPlaybackItem;
        }

        #endregion

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
    }
}