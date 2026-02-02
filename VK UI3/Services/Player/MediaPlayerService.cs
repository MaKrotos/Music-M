using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Animation;
using MusicX.Services.Player.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.DownloadTrack;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.Foundation;
using VK_UI3.Helpers;
using MusicX.Services.Player;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using VK_UI3.Services.Player;

namespace VK_UI3.Services
{
    public class MediaPlayerService
    {
        #region Constants and Static Fields

        public const int MEDIA_KEY_DEBOUNCE_MS = 250;
        public static readonly IEnumerable<ITrackMediaSource> _mediaSources = App._host.Services.GetRequiredService<IEnumerable<ITrackMediaSource>>();
        public static MediaPlayer _mediaPlayer = new MediaPlayer();
        public static IVKGetAudio _iVKGetAudio = null;
        public static AudioEqualizer _equalizer = null;
        public static ExtendedAudio _trackDataThis;
        public static ExtendedAudio _playingTrack = null;
        public static ExtendedAudio _nextTrack = null;
        public static CancellationTokenSource? _tokenSource;
        public static CancellationTokenSource? _nextTrackTokenSource;
        public static bool _isProcessingMediaKey = false;
        public static DateTime _lastMediaKeyTime = DateTime.MinValue;

        // Win32 API константы
        private const int WM_APPCOMMAND = 0x0319;
        private const int GWL_WNDPROC = -4;

        private static IntPtr _hwnd = IntPtr.Zero;
        private static MediaKeyHook _mediaKeyHook;
        private static bool _isInitialized = false;
        private static Window _mainWindow;

        #endregion

        #region Events

        public static event EventHandler oniVKUpdate;
        public static event EventHandler onClickonTrack;
        public static event EventHandler AudioPlayedChangeEvent;
        public static event EventHandler<VolumeChangedEventArgs> VolumeChanged;
        public static event EventHandler<bool> MediaKeyEnabledChanged;
        public static event EventHandler<TimeSpan> PositionChanged;

        #endregion

        #region Properties

        public static Windows.Media.Playback.MediaPlayer MediaPlayer
        {
            get { return _mediaPlayer; }
            set { _mediaPlayer = value; }
        }

        public static IVKGetAudio iVKGetAudio
        {
            get { return _iVKGetAudio; }
            set
            {
                if (_iVKGetAudio == value) return;
                _iVKGetAudio = value;
                NotifyoniVKUpdate();
            }
        }

        public static AudioEqualizer Equalizer
        {
            get { return _equalizer; }
            set
            {
                _equalizer = value;
                // Note: PlayTrack(position: mediaPlayer.Position); will need to be handled elsewhere
            }
        }

        public static ExtendedAudio PlayingTrack
        {
            get { return _playingTrack; }
            set { _playingTrack = value; }
        }

        public static bool IsMediaKeysEnabled { get; private set; } = true;

        private static double _volume = 1.0;
        public static double Volume
        {
            get => _volume;
            set
            {
                if (_volume != value)
                {
                    _volume = Math.Clamp(value, 0.0, 1.0);
                    _mediaPlayer.Volume = _volume;
                    VolumeChanged?.Invoke(null, new VolumeChangedEventArgs(_volume));
                }
            }
        }

        private static bool _isMuted = false;
        public static bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (_isMuted != value)
                {
                    _isMuted = value;
                    _mediaPlayer.IsMuted = _isMuted;
                    VolumeChanged?.Invoke(null, new VolumeChangedEventArgs(_volume, _isMuted));
                }
            }
        }

        #endregion

        #region Win32 Interop

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Initialization and Cleanup

        public static void Initialize(Window window)
        {
            if (_isInitialized) return;
            _mainWindow = window;

            // Получаем handle окна
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            if (hwnd == IntPtr.Zero) return;

            _hwnd = hwnd;

            // Настраиваем медиаплеер
            InitializeMediaPlayer();

            // Настраиваем SystemMediaTransportControls
            SetupSystemMediaTransportControls();

            // Инициализируем Win32 хук
            InitializeWin32MediaKeys(hwnd);

            _isInitialized = true;
        }

        private static void InitializeMediaPlayer()
        {
            try
            {
                // Настраиваем медиаплеер как в старом коде
                _mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;

                // ВКЛЮЧАЕМ CommandManager - это КРИТИЧЕСКИ важно!
                _mediaPlayer.CommandManager.IsEnabled = true;

                // Настраиваем поведение команд
                _mediaPlayer.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
                _mediaPlayer.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;
                _mediaPlayer.CommandManager.PlayBehavior.EnablingRule = MediaCommandEnablingRule.Always;
                _mediaPlayer.CommandManager.PauseBehavior.EnablingRule = MediaCommandEnablingRule.Always;

                // Подписываемся на события CommandManager
                _mediaPlayer.CommandManager.NextReceived += CommandManager_NextReceived;
                _mediaPlayer.CommandManager.PreviousReceived += CommandManager_PreviousReceived;
                _mediaPlayer.CommandManager.PauseReceived += CommandManager_PauseReceived;
                _mediaPlayer.CommandManager.PlayReceived += CommandManager_PlayReceived;

                // Подписываемся на другие события медиаплеера
                _mediaPlayer.VolumeChanged += OnMediaPlayerVolumeChanged;
                _mediaPlayer.IsMutedChanged += OnMediaPlayerMuteChanged;
                _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
                _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
                _mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                _mediaPlayer.SourceChanged += MediaPlayer_SourceChanged;

                // Подписываемся на события сессии
                _mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
                _mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing media player: {ex.Message}");
            }
        }

        private static void SetupSystemMediaTransportControls()
        {
            try
            {
                var systemControls = _mediaPlayer.SystemMediaTransportControls;

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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up system media controls: {ex.Message}");
            }
        }

        private static void SystemControls_ButtonPressed(SystemMediaTransportControls sender,
            SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            MainWindow.mainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        _mediaPlayer.Play();
                        break;
                    case SystemMediaTransportControlsButton.Pause:
                        _mediaPlayer.Pause();
                        break;
                    case SystemMediaTransportControlsButton.Next:
                        PlayNextTrack();
                        break;
                    case SystemMediaTransportControlsButton.Previous:
                        HandlePreviousTrack();
                        break;
                    case SystemMediaTransportControlsButton.Stop:
                        _mediaPlayer.Pause();
                        _mediaPlayer.Position = TimeSpan.Zero;
                        break;
                }
            });
        }

        private static void InitializeWin32MediaKeys(IntPtr hwnd)
        {
            try
            {
                _mediaKeyHook = new MediaKeyHook(hwnd, GWL_WNDPROC);
                _mediaKeyHook.MediaKeyPressed += OnMediaKeyPressed;
                _mediaKeyHook.VolumeKeyPressed += OnVolumeKeyPressed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize Win32 media keys: {ex.Message}");
            }
        }

        #endregion

        #region Media Player Event Handlers

        private static void MediaPlayer_MediaOpened(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            // Update track duration when a new track is loaded
            if (PlayingTrack?.audio?.Duration > 0)
            {
                // The AudioPlayer will update its TrackDurationMs property when AudioPlayedChangeEvent is raised
                NotifyAudioPlayedChange(PlayingTrack);
            }
        }

        private static void MediaPlayer_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            // Проверяем режим повтора
            var repeatMode = SettingsTable.GetSetting("playNext")?.settingValue ?? "RepeatAll";

            // Если режим "Повторять один", то перезапускаем текущий трек
            if (repeatMode == "RepeatOne")
            {
                // Перезапускаем текущий трек
                if (PlayingTrack != null)
                {
                    _ = PlayTrack();
                }
            }
            else
            {
                // Автопереход к следующему треку
                PlayNextTrack();
            }
        }

        private static void MediaPlayer_CurrentStateChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            // Обновить UI состояния воспроизведения
        }

        private static void MediaPlayer_SourceChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            // Обновить отображение текущего трека
            if (PlayingTrack != null)
            {
                NotifyAudioPlayedChange(PlayingTrack);
            }
        }

        private static void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            // Обновить позицию воспроизведения
            PositionChanged?.Invoke(null, sender.Position);
        }

        private static void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            // Обновить статус воспроизведения
        }

        #endregion

        #region Command Manager Event Handlers

        private static void CommandManager_NextReceived(MediaPlaybackCommandManager sender,
            MediaPlaybackCommandManagerNextReceivedEventArgs args)
        {
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                PlayNextTrack();
            });
        }

        private static void CommandManager_PreviousReceived(MediaPlaybackCommandManager sender,
            MediaPlaybackCommandManagerPreviousReceivedEventArgs args)
        {
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                HandlePreviousTrack();
            });
        }

        private static void CommandManager_PauseReceived(MediaPlaybackCommandManager sender,
            MediaPlaybackCommandManagerPauseReceivedEventArgs args)
        {
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                _mediaPlayer.Pause();
            });
        }

        private static void CommandManager_PlayReceived(MediaPlaybackCommandManager sender,
            MediaPlaybackCommandManagerPlayReceivedEventArgs args)
        {
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                _mediaPlayer.Play();
            });
        }

        #endregion

        #region Media Key Handlers

        private static void OnMediaKeyPressed(object sender, MediaKeyEventArgs e)
        {
            if (!IsMediaKeysEnabled || _isProcessingMediaKey) return;

            // Дебаунс для предотвращения множественных срабатываний
            var now = DateTime.Now;
            if ((now - _lastMediaKeyTime).TotalMilliseconds < MEDIA_KEY_DEBOUNCE_MS)
                return;

            _lastMediaKeyTime = now;

            HandleMediaKey(e.Key);
        }

        private static void OnVolumeKeyPressed(object sender, VolumeKeyEventArgs e)
        {
            if (!IsMediaKeysEnabled) return;

            switch (e.Key)
            {
                case VolumeKey.VolumeUp:
                    Volume = Math.Min(1.0, Volume + 0.05);
                    break;
                case VolumeKey.VolumeDown:
                    Volume = Math.Max(0.0, Volume - 0.05);
                    break;
                case VolumeKey.VolumeMute:
                    IsMuted = !IsMuted;
                    break;
            }
        }

        #endregion

        #region Cleanup

        public static void Cleanup()
        {
            System.Diagnostics.Debug.WriteLine("[MemoryLeakDebug] Cleanup started");
            if (!_isInitialized) return;

            _mediaKeyHook?.Dispose();

            if (_mediaPlayer != null)
            {
                // Отписываемся от CommandManager
                _mediaPlayer.CommandManager.NextReceived -= CommandManager_NextReceived;
                _mediaPlayer.CommandManager.PreviousReceived -= CommandManager_PreviousReceived;
                _mediaPlayer.CommandManager.PauseReceived -= CommandManager_PauseReceived;
                _mediaPlayer.CommandManager.PlayReceived -= CommandManager_PlayReceived;

                // Отписываемся от SystemMediaTransportControls
                var systemControls = _mediaPlayer.SystemMediaTransportControls;
                if (systemControls != null)
                {
                    systemControls.ButtonPressed -= SystemControls_ButtonPressed;
                }

                // Отписываемся от других событий
                _mediaPlayer.VolumeChanged -= OnMediaPlayerVolumeChanged;
                _mediaPlayer.IsMutedChanged -= OnMediaPlayerMuteChanged;
                _mediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
                _mediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
                _mediaPlayer.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
                _mediaPlayer.SourceChanged -= MediaPlayer_SourceChanged;

                _mediaPlayer.PlaybackSession.PositionChanged -= PlaybackSession_PositionChanged;
                _mediaPlayer.PlaybackSession.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;

                // Освобождаем ресурсы медиаплеера
                _mediaPlayer.Dispose();
                System.Diagnostics.Debug.WriteLine("[MemoryLeakDebug] MediaPlayer disposed");
            }

            _isInitialized = false;
            System.Diagnostics.Debug.WriteLine("[MemoryLeakDebug] Cleanup completed");
        }

        private static void OnMediaPlayerVolumeChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            _volume = sender.Volume;
            _isMuted = sender.IsMuted;
            VolumeChanged?.Invoke(null, new VolumeChangedEventArgs(_volume, _isMuted));
        }

        private static void OnMediaPlayerMuteChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            _isMuted = sender.IsMuted;
            VolumeChanged?.Invoke(null, new VolumeChangedEventArgs(_volume, _isMuted));
        }

        #endregion

        #region Media Key Handling

        private static async void HandleMediaKey(MediaKey key)
        {
            _isProcessingMediaKey = true;
            try
            {
                switch (key)
                {
                    case MediaKey.PlayPause:
                        await TogglePlayPause();
                        break;
                    case MediaKey.Next:
                        PlayNextTrack();
                        break;
                    case MediaKey.Previous:
                        HandlePreviousTrack();
                        break;
                    case MediaKey.Stop:
                        StopPlayback();
                        break;
                }
            }
            finally
            {
                await Task.Delay(MEDIA_KEY_DEBOUNCE_MS);
                _isProcessingMediaKey = false;
            }
        }

        public static async Task TogglePlayPause()
        {
            if (_mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                _mediaPlayer.Pause();
            }
            else
            {
                // Если трек не загружен, пытаемся загрузить текущий
                if (_mediaPlayer.Source == null && iVKGetAudio?.currentTrack != null)
                {
                    _ = PlayTrack();
                }
                else
                {
                    _mediaPlayer.Play();
                }
            }
        }

        public static void StopPlayback()
        {
            _mediaPlayer.Pause();
            _mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
        }

        public static void EnableMediaKeys(bool enable)
        {
            if (IsMediaKeysEnabled != enable)
            {
                IsMediaKeysEnabled = enable;
                MediaKeyEnabledChanged?.Invoke(null, enable);
            }
        }

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

        public static void NotifyAudioPlayedChange(ExtendedAudio trackdata)
        {

        }

        private static readonly object _trackSwitchLock = new object();
        private static DateTime _lastTrackSwitchTime = DateTime.MinValue;

        public static void PlayNextTrack()
        {
            // Более надежный debounce с блокировкой
            if (!Monitor.TryEnter(_trackSwitchLock))
            {
                System.Diagnostics.Debug.WriteLine("[TrackSwitch] PlayNextTrack blocked by lock");
                return;
            }

            try
            {
                var now = DateTime.Now;
                if ((now - _lastTrackSwitchTime).TotalMilliseconds < MEDIA_KEY_DEBOUNCE_MS)
                {
                    System.Diagnostics.Debug.WriteLine("[TrackSwitch] PlayNextTrack debounced");
                    return;
                }

                _lastTrackSwitchTime = now;

                MainWindow.mainWindow.DispatcherQueue.TryEnqueue(async () =>
                {
                    try
                    {
                        iVKGetAudio?.setNextTrackForPlay();
                        await PlayTrack();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[TrackSwitch] Error in PlayNextTrack: {ex.Message}");
                    }
                });
            }
            finally
            {
                Monitor.Exit(_trackSwitchLock);
            }
        }


        public static void PlayPreviousTrack()
        {
            // Используем ту же блокировку, что и для PlayNextTrack
            if (!Monitor.TryEnter(_trackSwitchLock))
            {
                System.Diagnostics.Debug.WriteLine("[TrackSwitch] PlayPreviousTrack blocked by lock");
                return;
            }

            try
            {
                var now = DateTime.Now;
                if ((now - _lastTrackSwitchTime).TotalMilliseconds < MEDIA_KEY_DEBOUNCE_MS)
                {
                    System.Diagnostics.Debug.WriteLine("[TrackSwitch] PlayPreviousTrack debounced");
                    return;
                }

                _lastTrackSwitchTime = now;

                MainWindow.mainWindow.DispatcherQueue.TryEnqueue(async () =>
                {
                    try
                    {
                        iVKGetAudio?.setPreviusTrackForPlay();
                        await PlayTrack();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[TrackSwitch] Error in PlayPreviousTrack: {ex.Message}");
                    }
                });
            }
            finally
            {
                Monitor.Exit(_trackSwitchLock);
            }
        }


        public static async void PlayList(IVKGetAudio userAudio)
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
            _ = PlayTrack();
        }

        #endregion

        #region Private Methods

        private static readonly object _previousTrackLock = new object();
        private static DateTime _lastPreviousTrackTime = DateTime.MinValue;

        /// <summary>
        /// Handles the previous track logic consistently across all input methods
        /// </summary>
        public static void HandlePreviousTrack()
        {
            lock (_previousTrackLock)
            {
                var now = DateTime.Now;

                // Проверяем, прошло ли достаточно времени с последнего вызова
                if ((now - _lastPreviousTrackTime).TotalMilliseconds < MEDIA_KEY_DEBOUNCE_MS)
                {
                    return; // Слишком частый вызов - игнорируем
                }

                _lastPreviousTrackTime = now;

                // Основная логика метода
                if (_mediaPlayer.Position.TotalSeconds >= 3)
                {
                    _mediaPlayer.Position = TimeSpan.Zero;
                    // Перезапускаем текущий трек
                    if (PlayingTrack != null)
                    {
                        _ = PlayTrack();
                    }
                }
                else
                {
                    PlayPreviousTrack();
                }
            }
        }

        private static readonly SemaphoreSlim _playTrackSemaphore = new SemaphoreSlim(1, 1);

        private async static Task PlayTrack(long? v = 0, TimeSpan? position = null)
        {
            // Ограничиваем количество одновременных вызовов PlayTrack
            if (!await _playTrackSemaphore.WaitAsync(TimeSpan.FromMilliseconds(500)))
            {
                System.Diagnostics.Debug.WriteLine("[TrackSwitch] PlayTrack semaphore timeout");
                return;
            }

            try
            {
                if (iVKGetAudio == null)
                    return;

                _tokenSource?.Cancel();
                _tokenSource?.Dispose();
                _tokenSource = new();

                // Отменяем предзагрузку предыдущего следующего трека
                _nextTrackTokenSource?.Cancel();
                _nextTrackTokenSource?.Dispose();
                _nextTrack = null;

                await EnsureTrackListLoaded();

                if (iVKGetAudio.listAudio.Count == 0) return;

                if (v != null && iVKGetAudio.currentTrack == null)
                    iVKGetAudio.currentTrack = (long)v;

                var trackdata = await _TrackDataThisGet(true);
                if (trackdata == null)
                    return;

                SendPlaybackStatistics(trackdata);
                SendVKStartEvent(trackdata);
                UpdateSystemMediaDisplay(PlayingTrack);

                if (ShouldSkipTrack(trackdata))
                {
                    PlayNextTrack();
                    return;
                }

                await LoadAndPlayTrack(trackdata, position);
            }
            finally
            {
                _playTrackSemaphore.Release();
            }
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
                previousTrackPlayedSeconds = (int)_mediaPlayer.Position.TotalSeconds;
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
            System.Diagnostics.Debug.WriteLine($"[TrackSwitch] LoadAndPlayTrack started for track: {trackdata.audio.Title}");

            // Освобождаем предыдущие ресурсы перед загрузкой нового трека
            if (_mediaPlayer.Source is MediaPlaybackItem oldItem)
            {
                oldItem.AudioTracksChanged -= MediaPlaybackItem_AudioTracksChanged;
                oldItem.TimedMetadataTracksChanged -= MediaPlaybackItem_TimedMetadataTracksChanged;
            }

            // Создаем новый CancellationToken для этой операции
            var loadCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                var mediaSource = Windows.Media.Core.MediaSource.CreateFromUri(new Uri(trackdata.audio.Url.ToString()));
                var mediaPlaybackItem = new Windows.Media.Playback.MediaPlaybackItem(mediaSource);

                // Подписываемся на события для корректной очистки
                mediaPlaybackItem.AudioTracksChanged += MediaPlaybackItem_AudioTracksChanged;
                mediaPlaybackItem.TimedMetadataTracksChanged += MediaPlaybackItem_TimedMetadataTracksChanged;

                PlayingTrack = trackdata;
                _mediaPlayer.Pause();

                System.Diagnostics.Debug.WriteLine($"[TrackSwitch] Before LoadWithMediaSources/LoadBasicMediaItem");

                if (new CheckFFmpeg().IsExist())
                {
                      System.Diagnostics.Debug.WriteLine($"[TrackSwitch] Using FFmpeg path");
                    await LoadWithMediaSources(trackdata);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[TrackSwitch] Using basic media item path");
                    LoadBasicMediaItem(trackdata, mediaPlaybackItem);
                }

                System.Diagnostics.Debug.WriteLine($"[TrackSwitch] After LoadWithMediaSources/LoadBasicMediaItem");

                if (position != null)
                {
                    _mediaPlayer.Position = new TimeSpan(0);
                }

                iVKGetAudio.ChangePlayAudio(trackdata);
                NotifyAudioPlayedChange(trackdata);

                // Обновить отображение в SystemMediaTransportControls
                UpdateSystemMediaDisplay(trackdata);

                _mediaPlayer.Play();

                System.Diagnostics.Debug.WriteLine($"[TrackSwitch] LoadAndPlayTrack completed for track: {trackdata.audio.Title}");

                // Начинаем предзагрузку следующего трека
                _ = PreloadNextTrack();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TrackSwitch] Error in LoadAndPlayTrack: {ex.Message}");
                throw;
            }
        }

        private static void UpdateSystemMediaDisplay(ExtendedAudio trackdata)
        {
            try
            {
                if (trackdata is null)
                    return;

                var systemControls = _mediaPlayer.SystemMediaTransportControls;
                var updater = systemControls.DisplayUpdater;

                updater.ClearAll();
                updater.Type = MediaPlaybackType.Music;
                updater.MusicProperties.Title = trackdata.audio.Title ?? "Unknown Title";
                updater.MusicProperties.Artist = trackdata.audio.Artist ?? "Unknown Artist";

                if (trackdata.audio.Album != null)
                {
                    updater.MusicProperties.AlbumTitle = trackdata.audio.Album.Title ?? string.Empty;
                }

                if (trackdata.audio.Album?.Thumb != null)
                {
                    var thumbnailUrl = trackdata.audio.Album.Thumb.Photo600 ??
                                     trackdata.audio.Album.Thumb.Photo300 ??
                                     trackdata.audio.Album.Thumb.Photo270;

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
                            System.Diagnostics.Debug.WriteLine($"Error setting thumbnail: {ex.Message}");
                        }
                    }
                }

                updater.Update();

                // Обновить статус воспроизведения
                systemControls.PlaybackStatus = _mediaPlayer.PlaybackSession.PlaybackState switch
                {
                    MediaPlaybackState.Playing => MediaPlaybackStatus.Playing,
                    MediaPlaybackState.Paused => MediaPlaybackStatus.Paused,
                    MediaPlaybackState.Buffering => MediaPlaybackStatus.Changing,
                    _ => MediaPlaybackStatus.Stopped
                };
                AudioPlayedChangeEvent?.Invoke(trackdata, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating system media display: {ex.Message}");
            }
        }

        private static async Task LoadWithMediaSources(ExtendedAudio trackdata)
        {
            System.Diagnostics.Debug.WriteLine($"[MemoryLeakDebug] LoadWithMediaSources started for track: {trackdata.audio.Title}");
            var allSourcesTask = Task.WhenAll(_mediaSources.Select(b =>
                b.OpenWithMediaPlayerAsync(_mediaPlayer, trackdata.audio, _tokenSource.Token, equalizer: _equalizer)));

            try
            {
                await allSourcesTask;
                System.Diagnostics.Debug.WriteLine($"[MemoryLeakDebug] LoadWithMediaSources completed for track: {trackdata.audio.Title}");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine($"[MemoryLeakDebug] LoadWithMediaSources failed for track: {trackdata.audio.Title}");
                if (allSourcesTask.IsCanceled ||
                    allSourcesTask.Exception?.InnerExceptions.All(b => b is OperationCanceledException) is true)
                    return;

                throw;
            }

            if (!allSourcesTask.Result.Any(b => b))
            {
                System.Diagnostics.Debug.WriteLine($"[MemoryLeakDebug] No sources succeeded for track: {trackdata.audio.Title}");
                PlayNextTrack();
                return;
            }
        }

        private static void LoadBasicMediaItem(ExtendedAudio trackdata, MediaPlaybackItem mediaPlaybackItem)
        {
            System.Diagnostics.Debug.WriteLine($"[MemoryLeakDebug] LoadBasicMediaItem started for track: {trackdata.audio.Title}");

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
            _mediaPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(1);
            // Note: MainWindow.mainWindow.requstDownloadFFMpegAsync(); will need to be handled elsewhere
            _mediaPlayer.Source = mediaPlaybackItem;

            System.Diagnostics.Debug.WriteLine($"[MemoryLeakDebug] LoadBasicMediaItem completed for track: {trackdata.audio.Title}");
        }

        public static async Task<ExtendedAudio> _TrackDataThisGet(bool forced = false)
        {
            if (iVKGetAudio != null && iVKGetAudio.countTracks != 0)
            {
                return await iVKGetAudio.GetTrackPlay(forced);
            }
            return _trackDataThis;
        }

        #endregion

        private static void MediaPlaybackItem_AudioTracksChanged(MediaPlaybackItem sender, Windows.Foundation.Collections.IVectorChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"[TrackSwitch] Audio tracks changed: {args.CollectionChange}");
        }

        private static void MediaPlaybackItem_TimedMetadataTracksChanged(MediaPlaybackItem sender, Windows.Foundation.Collections.IVectorChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"[TrackSwitch] Metadata tracks changed: {args.CollectionChange}");
        }

        private static async Task PreloadNextTrack()
        {
            try
            {
                // Создаем новый CancellationToken для предзагрузки
                _nextTrackTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                // Получаем следующий трек
                var nextTrack = await iVKGetAudio?.GetTrackPlay(false);
                if (nextTrack == null || string.IsNullOrEmpty(nextTrack.audio?.Url?.ToString()))
                {
                    return;
                }

                // Проверяем, не отменирован ли токен
                if (_nextTrackTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                // Сохраняем следующий трек для быстрой загрузки
                _nextTrack = nextTrack;
                System.Diagnostics.Debug.WriteLine($"[Preload] Next track preloaded: {nextTrack.audio.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Preload] Error preloading next track: {ex.Message}");
            }
        }
    }
}