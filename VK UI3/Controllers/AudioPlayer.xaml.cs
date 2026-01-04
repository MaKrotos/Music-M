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
        private static readonly IEnumerable<ITrackMediaSource> _mediaSources = App._host.Services.GetRequiredService<IEnumerable<ITrackMediaSource>>();
        public static Windows.Media.Playback.MediaPlayer mediaPlayer = new MediaPlayer();

        #endregion

        #region Events

        public static event EventHandler oniVKUpdate;
        public static event EventHandler onClickonTrack;
        public static event EventHandler AudioPlayedChangeEvent;

        #endregion

        #region Fields

        private DiscordRichPresenceManager discordRichPresenceManager = new DiscordRichPresenceManager();
        private static IVKGetAudio _iVKGetAudio = null;
        private static AudioEqualizer _equalizer = null;
        private static ExtendedAudio _trackDataThis;
        public static ExtendedAudio PlayingTrack = null;
        private static CancellationTokenSource? _tokenSource;
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

        #endregion

        #region Properties

        public MediaPlayer MediaPlayer
        {
            get { return mediaPlayer; }
            set { mediaPlayer = value; }
        }

        public static IVKGetAudio iVKGetAudio
        {
            get { return _iVKGetAudio; }
            set
            {
                if (_iVKGetAudio == value) return;
                _iVKGetAudio = value;
                MainView.mainView.setNewPlayingList(value);
                NotifyoniVKUpdate();
            }
        }

        public static AudioEqualizer Equalizer
        {
            get { return _equalizer; }
            set
            {
                _equalizer = value;
                PlayTrack(position: mediaPlayer.Position);
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
            get => mediaPlayer?.Volume * 100 ?? 100;
            set
            {
                var volume = value / 100;
                SettingsTable.SetSetting("Volume", volume.ToString());
                mediaPlayer.Volume = volume;
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
            InitializeWin32Hook();
        }

        private void InitializeEvents()
        {
            this.SizeChanged += RootGrid_SizeChanged;
            if (RootGrid != null)
                RootGrid.SizeChanged += RootGrid_SizeChanged;

            this.Loaded += AudioPlayer_Loaded;
            oniVKUpdate += AudioPlayer_oniVKUpdate;
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
                mediaPlayer.Volume = double.Parse(volumeSetting.settingValue);
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
                var systemControls = mediaPlayer.SystemMediaTransportControls;

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
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        mediaPlayer.Play();
                        break;
                    case SystemMediaTransportControlsButton.Pause:
                        mediaPlayer.Pause();
                        break;
                    case SystemMediaTransportControlsButton.Next:
                        PlayNextTrack();
                        break;
                    case SystemMediaTransportControlsButton.Previous:
                        if (mediaPlayer.Position.TotalSeconds >= 3)
                            mediaPlayer.Position = TimeSpan.Zero;
                        else
                            PlayPreviousTrack();
                        break;
                    case SystemMediaTransportControlsButton.Stop:
                        mediaPlayer.Pause();
                        mediaPlayer.Position = TimeSpan.Zero;
                        break;
                }
            });
        }

        private void UpdateSystemMediaDisplay()
        {
            if (TrackDataThis == null || TrackDataThis.audio == null) return;

            try
            {
                var systemControls = mediaPlayer.SystemMediaTransportControls;
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
                systemControls.PlaybackStatus = mediaPlayer.PlaybackSession.PlaybackState switch
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

        private void InitializeWin32Hook()
        {
            try
            {
                var window = Microsoft.UI.Xaml.Window.Current;
                if (window != null)
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    _wndProcDelegate = new WndProcDelegate(CustomWndProc);
                    _oldWndProc = SetWindowLongPtr(hwnd, GWL_WNDPROC, _wndProcDelegate);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up Win32 hook: {ex.Message}");
                // Если не удалось установить Win32 хук, используем только SystemMediaTransportControls
            }
        }

        private void InitializeMediaPlayer()
        {
            TrackDurationMs = 0;
            TrackPosition = 0;

            mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;

            // Включить и настроить CommandManager
            mediaPlayer.CommandManager.IsEnabled = true;

            mediaPlayer.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            mediaPlayer.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            mediaPlayer.CommandManager.PlayBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            mediaPlayer.CommandManager.PauseBehavior.EnablingRule = MediaCommandEnablingRule.Always;

            mediaPlayer.SystemMediaTransportControls.DisplayUpdater.Type = MediaPlaybackType.Music;

            // Media player events
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            mediaPlayer.SourceChanged += MediaPlayer_SourceChanged;

            // Playback session events
            mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
            mediaPlayer.PlaybackSession.BufferedRangesChanged += PlaybackSession_BufferedRangesChanged;
            mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;

            // Command manager events
            mediaPlayer.CommandManager.NextReceived += CommandManager_NextReceived;
            mediaPlayer.CommandManager.PreviousReceived += CommandManager_PreviousReceived;
            mediaPlayer.CommandManager.PauseReceived += CommandManager_PauseReceived;
            mediaPlayer.CommandManager.PlayReceived += CommandManager_PlayReceived;
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
                PlayTrack();
            else
                PlayNextTrack();
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
                        PlayTrack(iVKGetAudio?.currentTrack);
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

        #region Command Events

        private void CommandManager_NextReceived(MediaPlaybackCommandManager sender,
            MediaPlaybackCommandManagerNextReceivedEventArgs args)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                PlayNextTrack();
            });
        }

        private void CommandManager_PreviousReceived(MediaPlaybackCommandManager sender,
            MediaPlaybackCommandManagerPreviousReceivedEventArgs args)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (mediaPlayer.Position.TotalSeconds >= 3)
                {
                    mediaPlayer.Position = TimeSpan.Zero;
                }
                else
                {
                    PlayPreviousTrack();
                }
            });
        }

        private void CommandManager_PauseReceived(MediaPlaybackCommandManager sender,
            MediaPlaybackCommandManagerPauseReceivedEventArgs args)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                mediaPlayer.Pause();
            });
        }

        private void CommandManager_PlayReceived(MediaPlaybackCommandManager sender,
            MediaPlaybackCommandManagerPlayReceivedEventArgs args)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                mediaPlayer.Play();
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
            if (PlayingTrack == null) return;
            FlyOutControl.ShowAt(sender as Button);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NotifyonClickonTrack();
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
                mediaPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(e.NewValue);
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
            oniVKUpdate?.Invoke(null, EventArgs.Empty);
        }

        public static void NotifyonClickonTrack()
        {
            if (onClickonTrack != null && PlayingTrack != null)
            {
                onClickonTrack.Invoke(PlayingTrack, EventArgs.Empty);
            }
        }

        internal static void PlayNextTrack()
        {
            Task.Run(async () =>
            {
                iVKGetAudio?.setNextTrackForPlay();
                PlayTrack();
            });
        }

        internal static void PlayPreviousTrack()
        {
            Task.Run(async () =>
            {
                iVKGetAudio?.setPreviusTrackForPlay();
                PlayTrack();
            });
        }

        internal static async void PlayList(IVKGetAudio userAudio)
        {
            if (userAudio.listAudio.Count == 0)
            {
                var tcs = new TaskCompletionSource<bool>();
                EventHandler handler = null;
                handler = (sender, args) =>
                {
                    userAudio.onListUpdate -= handler;
                    tcs.SetResult(true);
                };
                userAudio.onListUpdate += handler;

                userAudio.GetTracks();
                await tcs.Task;
            }

            if (userAudio.listAudio.Count == 0) return;

            iVKGetAudio = userAudio;
            AudioPlayer.PlayTrack();
        }

        #endregion

        #region Private Methods

        #region Player Control Methods

        private async static Task PlayTrack(long? v = 0, TimeSpan? position = null)
        {
            if (iVKGetAudio == null)
                return;

            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new();

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
            ExtendedAudio previousTrack = PlayingTrack;
            int? previousTrackPlayedSeconds = null;

            if (previousTrack != null)
            {
                previousTrackPlayedSeconds = (int)mediaPlayer.Position.TotalSeconds;
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

            PlayingTrack = trackdata;
            mediaPlayer.Pause();

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
                mediaPlayer.Position = (TimeSpan)position;
            }

            mediaPlayer.Play();
            iVKGetAudio.ChangePlayAudio(trackdata);
            AudioPlayedChangeEvent?.Invoke(trackdata, EventArgs.Empty);
        }

        private static async Task LoadWithMediaSources(ExtendedAudio trackdata)
        {
            var allSourcesTask = Task.WhenAll(_mediaSources.Select(b =>
                b.OpenWithMediaPlayerAsync(mediaPlayer, trackdata.audio, _tokenSource.Token, equalizer: _equalizer)));

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
            mediaPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(1);
            MainWindow.mainWindow.requstDownloadFFMpegAsync();
            mediaPlayer.Source = mediaPlaybackItem;
        }

        #endregion

        #region UI Helper Methods

        private void TogglePlayPause()
        {
            switch (mediaPlayer.CurrentState)
            {
                case MediaPlayerState.Playing:
                    mediaPlayer.Pause();
                    break;
                case MediaPlayerState.Paused:
                    mediaPlayer.Play();
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
            if (highlightSetting != null && highlightSetting.settingValue.Equals("1") && PlayingTrack != null)
            {
                PlayingTrack.iVKGetAudio.SelectAudio(PlayingTrack);
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

            discordRichPresenceManager.SetTrack(TrackDataThis, mediaPlayer);
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
            return _trackDataThis;
        }

        #endregion

        #endregion

        #region Win32 Interop

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, WndProcDelegate newProc);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint newProc);

        [DllImport("user32.dll")]
        private static extern nint CallWindowProc(nint lpPrevWndFunc, nint hWnd, int msg, nint wParam, nint lParam);

        private static int HIWORD(nint n) => (int)((n >> 16) & 0xFFFF);

        private nint CustomWndProc(nint hwnd, int msg, nint wParam, nint lParam)
        {
            const int APPCOMMAND_MASK = 0xF000;

            if (msg == WM_APPCOMMAND)
            {
                int cmd = HIWORD(wParam) & ~APPCOMMAND_MASK;

                this.DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        switch (cmd)
                        {
                            case 8:  // APPCOMMAND_VOLUME_MUTE
                                mediaPlayer.IsMuted = !mediaPlayer.IsMuted;
                                break;
                            case 9:  // APPCOMMAND_VOLUME_DOWN
                                mediaPlayer.Volume = Math.Max(0, mediaPlayer.Volume - 0.05);
                                break;
                            case 10: // APPCOMMAND_VOLUME_UP
                                mediaPlayer.Volume = Math.Min(1, mediaPlayer.Volume + 0.05);
                                break;
                            case 46: // APPCOMMAND_MEDIA_PLAY_PAUSE
                                TogglePlayPause();
                                break;
                            case 11: // APPCOMMAND_MEDIA_NEXTTRACK
                                PlayNextTrack();
                                break;
                            case 12: // APPCOMMAND_MEDIA_PREVIOUSTRACK
                                if (mediaPlayer.Position.TotalSeconds >= 3)
                                    mediaPlayer.Position = TimeSpan.Zero;
                                else
                                    PlayPreviousTrack();
                                break;
                            case 13: // APPCOMMAND_MEDIA_STOP
                                mediaPlayer.Pause();
                                mediaPlayer.Position = TimeSpan.Zero;
                                break;
                            case 47: // APPCOMMAND_MEDIA_PLAY
                                mediaPlayer.Play();
                                break;
                            case 48: // APPCOMMAND_MEDIA_PAUSE
                                mediaPlayer.Pause();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error handling media key: {ex.Message}");
                    }
                });

                return IntPtr.Zero;
            }

            if (_oldWndProc != 0)
                return CallWindowProc(_oldWndProc, hwnd, msg, wParam, lParam);

            return IntPtr.Zero;
        }

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