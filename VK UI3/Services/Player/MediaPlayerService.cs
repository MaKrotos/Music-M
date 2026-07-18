using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MusicX.Services.Player;
using MusicX.Services.Player.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.DownloadTrack;
using VK_UI3.Helpers;
using VK_UI3.Models;
using VK_UI3.Services.Player;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;

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

        private static bool _isInitialized = false;
        private static Window _mainWindow;

        #endregion

        #region Events

        public static event EventHandler oniVKUpdate;
        public static event EventHandler onClickonTrack;
        public static event EventHandler AudioPlayedChangeEvent;
        public static event EventHandler<VolumeChangedEventArgs> VolumeChanged;
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

        #region Initialization and Cleanup

        public static void Initialize(Window window)
        {
            if (_isInitialized) return;
            _mainWindow = window;

            // Настраиваем медиаплеер
            InitializeMediaPlayer();

            // Настраиваем SystemMediaTransportControls
            SetupSystemMediaTransportControls();

            _isInitialized = true;
        }

        private static void InitializeMediaPlayer()
        {
            try
            {
                // Настраиваем медиаплеер как в старом коде
                _mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;

                // ВКЛЮЧАЕМ CommandManager — это КРИТИЧЕСКИ важно для глобальной обработки
                // медиа-клавиш через SystemMediaTransportControls, даже когда окно свёрнуто.
                // Когда CommandManager включён, Windows сама маршрутизирует медиа-клавиши
                // в приложение через SMTC, независимо от состояния окна.
                _mediaPlayer.CommandManager.IsEnabled = true;

                // Настраиваем правила включения для всех медиа-команд
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

        #region Cleanup

        public static void Cleanup()
        {
            System.Diagnostics.Debug.WriteLine("[MemoryLeakDebug] Cleanup started");
            if (!_isInitialized) return;

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
            AudioPlayedChangeEvent?.Invoke(trackdata, EventArgs.Empty);
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
                _mediaPlayer.Position = new TimeSpan(0);
                _mediaPlayer.Source = null;

                await EnsureTrackListLoaded();

                if (iVKGetAudio.listAudio.Count == 0) return;

                if (v != null && iVKGetAudio.currentTrack == null)
                    iVKGetAudio.currentTrack = (long)v;

                var trackdata = await _TrackDataThisGet(true);
                if (trackdata == null)
                    return;

                SendPlaybackStatistics(trackdata);
                SendVKStartEvent(trackdata);
                SetVKStatus(trackdata);
                UpdateSystemMediaDisplay(PlayingTrack);

                if (ShouldSkipTrack(trackdata))
                {
                    PlayNextTrack();
                    return;
                }

                try
                {
                    await LoadAndPlayTrack(trackdata, position);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[TrackSwitch] Error in PlayTrack loading track: {ex.Message}");
                    // Не пробрасываем исключение дальше — приложение не должно падать
                }
            }
            finally
            {
                _playTrackSemaphore.Release();
            }
        }




        private async static void SetVKStatus(ExtendedAudio trackdata)
        {
            var share = SettingsTable.GetSetting("shareFriend");
            if (share.settingValue == "true" && trackdata != null)

                await VK.api.Audio.SetBroadcastAsync(
                   trackdata.audio.OwnerId + "_" + trackdata.audio.Id
                );

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
            return trackdata?.audio == null ||
                   trackdata.audio.Url == null ||
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

                bool ffmpegExists = new CheckFFmpeg().IsExist();
                bool useFfmpeg = true;
                var setting = SettingsTable.GetSetting("useFfmpegForPlayback");
                if (setting != null && setting.settingValue == "0")
                {
                    useFfmpeg = false;
                }

                if (ffmpegExists && useFfmpeg)
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
                //_ = PreloadNextTrack();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TrackSwitch] Error in LoadAndPlayTrack: {ex.Message}");
                // Не пробрасываем исключение — приложение не должно падать
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MemoryLeakDebug] LoadWithMediaSources failed for track: {trackdata.audio.Title}: {ex.Message}");
                if (allSourcesTask.IsCanceled ||
                    allSourcesTask.Exception?.InnerExceptions.All(b => b is OperationCanceledException) is true)
                    return;

                // Не пробрасываем исключение — приложение не должно падать
                return;
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

        // Debounce для предотвращения дублирования WM_APPCOMMAND
        // между глобальным хуком (GlobalMediaKeyHook) и оконной процедурой (NewWindowProc).
        private static readonly object _appCommandLock = new object();
        private static DateTime _lastAppCommandTime = DateTime.MinValue;

        /// <summary>
        /// Обрабатывает WM_APPCOMMAND (медиа-клавиши с клавиатуры).
        /// Вызывается из MainWindow.NewWindowProc и GlobalMediaKeyHook.
        /// </summary>
        public static void HandleAppCommand(int cmd)
        {
            // Debounce: если команда была обработана менее 300 мс назад — игнорируем.
            // Это защита от дублирования, когда WM_APPCOMMAND приходит и через глобальный хук,
            // и через оконную процедуру одновременно.
            lock (_appCommandLock)
            {
                var now = DateTime.Now;
                if ((now - _lastAppCommandTime).TotalMilliseconds < MEDIA_KEY_DEBOUNCE_MS)
                {
                    System.Diagnostics.Debug.WriteLine($"[MediaPlayerService] WM_APPCOMMAND cmd={cmd} debounced (too frequent)");
                    return;
                }
                _lastAppCommandTime = now;
            }

            // Проверяем, не назначена ли эта медиа-клавиша в HotkeyService.
            // Если назначена - пропускаем, т.к. она будет обработана через WM_HOTKEY.
            if (HotkeyService.Instance != null)
            {
                int vkCode = AppCommandToVkCode(cmd);
                if (vkCode != 0 && HotkeyService.Instance.IsMediaKeyRegistered(vkCode))
                {
                    System.Diagnostics.Debug.WriteLine($"[MediaPlayerService] WM_APPCOMMAND cmd={cmd} skipped - handled by HotkeyService (VK=0x{vkCode:X2})");
                    return;
                }
            }

            MediaKey? key = null;

            switch (cmd)
            {
                case 8:  // APPCOMMAND_VOLUME_MUTE
                    IsMuted = !IsMuted;
                    return;
                case 9:  // APPCOMMAND_VOLUME_DOWN
                    Volume = Math.Max(0.0, Volume - 0.05);
                    return;
                case 10: // APPCOMMAND_VOLUME_UP
                    Volume = Math.Min(1.0, Volume + 0.05);
                    return;
                case 11: // APPCOMMAND_MEDIA_NEXTTRACK
                    key = MediaKey.Next;
                    break;
                case 12: // APPCOMMAND_MEDIA_PREVIOUSTRACK
                    key = MediaKey.Previous;
                    break;
                case 13: // APPCOMMAND_MEDIA_STOP
                    key = MediaKey.Stop;
                    break;
                case 14: // APPCOMMAND_MEDIA_PLAY_PAUSE
                case 46: // APPCOMMAND_MEDIA_PLAY
                case 47: // APPCOMMAND_MEDIA_PAUSE
                    key = MediaKey.PlayPause;
                    break;
            }

            if (key.HasValue)
            {
                HandleMediaKey(key.Value);
            }
        }

        /// <summary>
        /// Преобразует APPCOMMAND код в Win32 VirtualKey код.
        /// </summary>
        private static int AppCommandToVkCode(int cmd)
        {
            return cmd switch
            {
                8  => 0xB4, // APPCOMMAND_VOLUME_MUTE → VK_VOLUME_MUTE
                9  => 0xB6, // APPCOMMAND_VOLUME_DOWN → VK_VOLUME_DOWN
                10 => 0xB5, // APPCOMMAND_VOLUME_UP → VK_VOLUME_UP
                11 => 0xB2, // APPCOMMAND_MEDIA_NEXTTRACK → VK_MEDIA_NEXT_TRACK
                12 => 0xB1, // APPCOMMAND_MEDIA_PREVIOUSTRACK → VK_MEDIA_PREV_TRACK
                13 => 0xB3, // APPCOMMAND_MEDIA_STOP → VK_MEDIA_STOP
                14 or 46 or 47 => 0xB0, // APPCOMMAND_MEDIA_PLAY_PAUSE/PLAY/PAUSE → VK_MEDIA_PLAY_PAUSE
                _ => 0
            };
        }

        /// <summary>
        /// Выполняет указанное действие плеера. Используется горячими клавишами.
        /// </summary>
        public static void ExecuteAction(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.PlayPause:
                    _ = TogglePlayPause();
                    break;

                case PlayerAction.NextTrack:
                    PlayNextTrack();
                    break;

                case PlayerAction.PreviousTrack:
                    HandlePreviousTrack();
                    break;

                case PlayerAction.VolumeUp:
                    Volume = Math.Min(1.0, Volume + 0.05);
                    break;

                case PlayerAction.VolumeDown:
                    Volume = Math.Max(0.0, Volume - 0.05);
                    break;

                case PlayerAction.Mute:
                    IsMuted = !IsMuted;
                    break;

                case PlayerAction.Stop:
                    StopPlayback();
                    break;
            }
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