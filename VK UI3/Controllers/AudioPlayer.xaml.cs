//using CSCore.CoreAudioAPI;
using FFMediaToolkit;
using FFMediaToolkit.Audio;
using FFMediaToolkit.Decoding;
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using WinRT;
using VK_UI3.Views.ModalsPages;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controllers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    public sealed partial class AudioPlayer : Microsoft.UI.Xaml.Controls.Page, INotifyPropertyChanged
    /// </summary>
    {
        private static readonly IEnumerable<ITrackMediaSource> _mediaSources= App._host.Services.GetRequiredService<IEnumerable<ITrackMediaSource>>();

        public static Windows.Media.Playback.MediaPlayer mediaPlayer = new MediaPlayer();

        public static event EventHandler oniVKUpdate;

        public static event EventHandler onClickonTrack;

        DiscordRichPresenceManager discordRichPresenceManager = new DiscordRichPresenceManager();

        public static void NotifyoniVKUpdate()
        {
            oniVKUpdate.Invoke(null, EventArgs.Empty);
        }


        public static void NotifyonClickonTrack()
        {
            if (onClickonTrack != null && PlayingTrack != null)
            {
                onClickonTrack.Invoke(PlayingTrack, EventArgs.Empty);
            }
        }



        public MediaPlayer MediaPlayer
        {
            get { return mediaPlayer; }
            set { mediaPlayer = value; }
        }



        private long _trackPositionMs;
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

        private long _trackDurationMs;
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

        // Оставляем старые свойства для совместимости с UI, если нужно
        public int TrackPosition
        {
            get { return (int)(_trackPositionMs / 1000); }
            set { TrackPositionMs = value * 1000; }
        }
        


        public string Thumbnail
        {
            get
            {
                if (TrackDataThis.audio == null || TrackDataThis.audio.Album == null || (_TrackDataThisGet().Result).audio.Album.Thumb == null) return "null";
                if (TrackDataThis.audio.Album == null) return "null";
                return TrackDataThis.audio.Album.Thumb.Photo600
                     ?? TrackDataThis.audio.Album.Thumb.Photo300
                     ?? TrackDataThis.audio.Album.Thumb.Photo270
                     ?? TrackDataThis.audio.Album.Thumb.Photo68
                     ?? TrackDataThis.audio.Album.Thumb.Photo34
                     ?? "null";
            }

        }




        public event PropertyChangedEventHandler PropertyChanged;

        private double _redFillPercent = 0; 
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


        protected void OnPropertyChanged(string propertyName)
        {
            try
            {
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                });
            }
            catch
            { }
        }



        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        //  AudioSessionControl contr = null;
        AnimationsChangeFontIcon animateFontIcon = null;
        public AudioPlayer()
        {
            this.InitializeComponent();
            this.SizeChanged += RootGrid_SizeChanged;
            if (RootGrid != null)
                RootGrid.SizeChanged += RootGrid_SizeChanged;
            AnimateRedRectangle();


            changeIconPlayBTN = new AnimationsChangeFontIcon(this.PlayBTN, this.DispatcherQueue);
            animateFontIcon = new AnimationsChangeFontIcon(this.repeatBTNIcon, this.DispatcherQueue);
            changeImage = new AnimationsChangeImage(this.ImageThumb, DispatcherQueue);
            changeText = new AnimationsChangeText(ArtistTextBlock, this.DispatcherQueue);
            changeText2 = new AnimationsChangeText(TitleTextBlock, this.DispatcherQueue);
            statusAnimate = new AnimationsChangeFontIcon(StatusBTNIcon, this.DispatcherQueue);

            var setting = SettingsTable.GetSetting("Volume");
            if (setting != null)
            {
                mediaPlayer.Volume = double.Parse(setting.settingValue);
            }
            setStatusIcon();


            oniVKUpdate += AudioPlayer_oniVKUpdate;

            this.Loaded += AudioPlayer_Loaded;

            mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
            mediaPlayer.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            mediaPlayer.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            mediaPlayer.SystemMediaTransportControls.DisplayUpdater.Type = MediaPlaybackType.Music;


            TrackDurationMs = 0;
            TrackPosition = 0;

            TrackDataThisChanged.AddHandler(AudioPlayer_PropertyChanged);

            // Привязка к событию MediaOpened
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;

            // Привязка к событию MediaEnded
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            // Привязка к событию MediaFailed
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;

            // Привязка к событию CurrentStateChanged
            mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;

            // Привязка к событию SourceChanged
            mediaPlayer.SourceChanged += MediaPlayer_SourceChanged;
            mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
            mediaPlayer.PlaybackSession.BufferedRangesChanged += PlaybackSession_BufferedRangesChanged;


            mediaPlayer.CommandManager.NextReceived += CommandManager_NextReceived;
            mediaPlayer.CommandManager.PreviousReceived += CommandManager_PreviousReceived;
            mediaPlayer.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            mediaPlayer.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;

            // Восстановление эквалайзера из настроек
            try
            {
                var enabledSetting = VK_UI3.DB.SettingsTable.GetSetting("Equalizer_Enabled", "1");
                if (enabledSetting.settingValue == "1")
                {
                    var eqControl = new EqualizerControl();
                    eqControl.LoadSettings();
                    AudioEqualizer eq = eqControl.getEqualizes();
                    if (eq != null)
                        Equalizer = eq;
                }
                else
                {
                    Equalizer = null;
                }
            }
            catch { /* ignore errors */ }
        }
        double actualHeight = 0;



        public double simpleAudioBind
        {
            get
            {
                if (mediaPlayer == null) return 100;
                return mediaPlayer.Volume * 100;
            }
            set
            {
                var a = value / 100;
                SettingsTable.SetSetting("Volume", a.ToString());
                mediaPlayer.Volume = a;
            }
        }


        private void AudioPlayer_oniVKUpdate(object sender, EventArgs e)
        {
            //   pageRa.Height = actualHeight;
            DisableAllChildren(this, true);
            setButtonPlayNext();

        }

        private void AudioPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            actualHeight = pageRa.ActualHeight;
            if (iVKGetAudio == null)
            {
                DisableAllChildren(this);
                //   pageRa.Height = 0;
            }

        }


        bool enablinUI = false;
        void DisableAllChildren(DependencyObject parent, bool enable = false)
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


        private void CommandManager_PreviousReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPreviousReceivedEventArgs args)
        {
            if (mediaPlayer.Position.TotalSeconds >= 3)
            {
                mediaPlayer.Position = new TimeSpan(0);
            }
            else
            {
                PlayPreviousTrack();
            }
        }

        private void CommandManager_NextReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerNextReceivedEventArgs args)
        {
            PlayNextTrack();
        }



        private void PlaybackSession_BufferedRangesChanged(MediaPlaybackSession sender, object args)
        {
            if (TrackDataThis == null) return;
            var a = (int)Math.Round(sender.NaturalDuration.TotalSeconds);
            var b = TrackDataThis.audio.Duration;

            if (Math.Abs(a - b) > 5)
            {
                PlayTrack();
            }

            //  throw new NotImplementedException();
        }

        private void AudioPlayer_PropertyChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(TrackDataThis));
        }

        private void AudioPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Symbol.Audio;
        }

        private void MediaPlayer_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {

            if (SettingsTable.GetSetting("playNext").settingValue.Equals("RepeatOne"))
                PlayTrack();
            else
                PlayNextTrack();
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
                // SliderPositionMs не трогаем, пользователь двигает слайдер
            }
        }

        private void MediaPlayer_MediaOpened(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            // Код для выполнения при открытии медиафайла
            TrackDurationMs = (long)(TrackDataThis.audio.Duration * 1000);
        }
        private void MediaPlayer_MediaFailed(Windows.Media.Playback.MediaPlayer sender, Windows.Media.Playback.MediaPlayerFailedEventArgs args)
        {
            // Код для выполнения при ошибке воспроизведения медиафайла
        }
        Helpers.Animations.AnimationsChangeFontIcon changeIconPlayBTN = null;
        private void MediaPlayer_CurrentStateChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            // Код для выполнения при изменении состояния медиаплеера
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                switch (sender.CurrentState)
                {
                    case (MediaPlayerState)Windows.Media.Playback.MediaPlaybackState.Playing:

                        changeIconPlayBTN.ChangeFontIconWithAnimation("\uE769");
                        break;
                    case (MediaPlayerState)Windows.Media.Playback.MediaPlaybackState.Paused:
                        changeIconPlayBTN.ChangeFontIconWithAnimation("\uE768");

                        break;
                    case (MediaPlayerState)Windows.Media.Playback.MediaPlaybackState.Buffering:

                        break;
                    case MediaPlayerState.Closed:
                        PlayTrack(iVKGetAudio.currentTrack);
                        break;
                    default:
                        changeIconPlayBTN.ChangeFontIconWithAnimation("\uE895");
                        break;
                }
            });

        }

        public Storyboard storyboard = new Storyboard();
        Symbol? symbolNow = null;




        AnimationsChangeImage changeImage = null;
        AnimationsChangeText changeText = null;
        AnimationsChangeText changeText2 = null;



        private void MediaPlayer_SourceChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            if (TrackDataThis == null) return;

            var source = sender.Source as Windows.Media.Playback.MediaPlaybackItem;

            TrackDurationMs = (long)(TrackDataThis.audio.Duration * 1000);


            OnPropertyChanged(nameof(TrackPosition));
            OnPropertyChanged(nameof(TrackDataThis));

            changeImage.ChangeImageWithAnimation(Thumbnail);
            changeText.ChangeTextWithAnimation(TrackDataThis.audio.Artist);
            changeText2.ChangeTextWithAnimation(TrackDataThis.audio.Title);

            FlyOutControl.dataTrack = TrackDataThis;


            if (bool.Parse(SettingsTable.GetSetting("shareFriend").settingValue))
                iVKGetAudio.shareToVK();



            UpdateDiscordState();
        }

        private void UpdateDiscordState()
        {
            var setting = DB.SettingsTable.GetSetting("DisableDiscordIntegration");
            if (setting != null && setting.settingValue.Equals("1"))
                return;
            discordRichPresenceManager.SetTrack(TrackDataThis, mediaPlayer);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (isManualChange)
            {
                SliderPositionMs = (long)e.NewValue;
                mediaPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(e.NewValue);
            }
        }
        bool isManualChange = false;


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
            // После выхода синхронизируем слайдер с текущей позицией
            SliderPositionMs = TrackPositionMs;
        }





        private void Button_Play_Tapped(object sender, TappedRoutedEventArgs e)
        {

            switch (mediaPlayer.CurrentState)
            {
                case (MediaPlayerState)MediaPlaybackState.Playing:

                    mediaPlayer.Pause();
                    break;
                case (MediaPlayerState)MediaPlaybackState.Paused:
                    mediaPlayer.Play();

                    break;
                case (MediaPlayerState)MediaPlaybackState.Buffering:

                    break;


                default:
                    break;
            }

        }

        private static IVKGetAudio _iVKGetAudio = null;
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




        internal static void PlayNextTrack()
        {
            // Проверяем, не является ли текущий трек последним в списке

            Task.Run(async () =>
            {
                //iVKGetAudio.currentTrack++;
                iVKGetAudio.setNextTrackForPlay();
                PlayTrack();
            });

        }

        internal static void PlayPreviousTrack()
        {
            Task.Run(async () =>
            {
                iVKGetAudio.setPreviusTrackForPlay();
                PlayTrack();
            });
        }
        public static ExtendedAudio PlayingTrack = null;


        private static CancellationTokenSource? _tokenSource;

        private async static Task PlayTrack(long? v = 0, TimeSpan? position = null)
        {
            if (iVKGetAudio == null)
                return;

            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new();

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

            if (iVKGetAudio.listAudio.Count == 0) return;

            if (v != null && iVKGetAudio.currentTrack == null) 
                iVKGetAudio.currentTrack = (long)v;

           

            var trackdata = await _TrackDataThisGet(true);
            if (trackdata == null) 
                return;
     
            if (iVKGetAudio is PlayListVK)
            {
                VK.sendStartEvent((long)trackdata.audio.Id, (long)trackdata.audio.OwnerId, (iVKGetAudio as PlayListVK).playlist.Id);
            }
            else
                VK.sendStartEvent((long)trackdata.audio.Id, (long)trackdata.audio.OwnerId);

            if (trackdata.audio.Url == null || new DB.SkipPerformerDB().skipIsSet(trackdata.audio.Artist))
            {

                PlayNextTrack();
                return;
            }
            var mediaSource = Windows.Media.Core.MediaSource.CreateFromUri(new Uri(trackdata.audio.Url.ToString()));
            var mediaPlaybackItem = new Windows.Media.Playback.MediaPlaybackItem(mediaSource);

            ExtendedAudio preTrack = null;
            int? secDurPre = null;
            if (PlayingTrack != null)
            {
                preTrack = PlayingTrack;
                secDurPre = (int) mediaPlayer.Position.TotalSeconds;
            }
            PlayingTrack = trackdata;

            
            mediaPlayer.Pause();


            if (new CheckFFmpeg().IsExist())
            {

                var allSourcesTask = Task.WhenAll(_mediaSources.Select(b => b.OpenWithMediaPlayerAsync(mediaPlayer, trackdata.audio, _tokenSource.Token, equalizer: _equalizer)));

                try
                {
                    await allSourcesTask;
                }
                catch
                {
                    // await unwraps AggregateException into only the first exception,
                    // but we need to make sure that all exceptions are cancel ones
                    if (allSourcesTask.IsCanceled || allSourcesTask.Exception?.InnerExceptions.All(b => b is OperationCanceledException) is true)
                        return; // canceled

                    throw;
                }

                if (!allSourcesTask.Result.Any(b => b)) // no sources picked up this track
                {
                    PlayNextTrack();
                    return;
                }


            }
            else
            {
                MediaItemDisplayProperties props = mediaPlaybackItem.GetDisplayProperties();
                props.Type = Windows.Media.MediaPlaybackType.Music;
                props.MusicProperties.Title = trackdata.audio.Title;
                props.MusicProperties.AlbumArtist = trackdata.audio.Artist;


                if (trackdata.audio.Album != null && trackdata.audio.Album.Thumb != null)
                {
                    RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromUri(new Uri(
                        trackdata.audio.Album.Thumb.Photo600 ??
                        trackdata.audio.Album.Thumb.Photo270 ??
                        trackdata.audio.Album.Thumb.Photo300
                        ));

                    props.Thumbnail = imageStreamRef;
                    mediaPlaybackItem.ApplyDisplayProperties(props);

                    props.Thumbnail = imageStreamRef;

                }
                else
                    props.Thumbnail = null;
                mediaPlaybackItem.ApplyDisplayProperties(props);

                mediaPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(1);
                MainWindow.mainWindow.requstDownloadFFMpegAsync();
                mediaPlayer.Source = mediaPlaybackItem;
            }

            if (position != null)
            {
                mediaPlayer.Position = (TimeSpan)position;
            }
            mediaPlayer.Play();

            iVKGetAudio.ChangePlayAudio(trackdata);
            

            if (AudioPlayedChangeEvent != null)
            AudioPlayedChangeEvent.Invoke(trackdata, EventArgs.Empty);


        }


        public static event EventHandler AudioPlayedChangeEvent;



        private void PreviousBTN_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PlayPreviousTrack();
        }

        private void goToPlayList_BTN(object sender, TappedRoutedEventArgs e)
        {
            MainView.mainView.TogglePlayNowPanel();
        }

        public void OnDisplayNameChanged(string newDisplayName, ref Guid eventContext)
        {

        }

        public void OnIconPathChanged(string newIconPath, ref Guid eventContext)
        {

        }

        public void OnSimpleVolumeChanged(float volume, bool isMuted)
        {
            Console.WriteLine($"Volume changed to {volume}, muted: {isMuted}");
        }


        public void OnChannelVolumeChanged(int channelCount, float[] newChannelVolumeArray, int changedChannel, ref Guid eventContext)
        {

        }

        public void OnGroupingParamChanged(ref Guid newGroupingParam, ref Guid eventContext)
        {

        }



        private void SoundSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //  if (simpleAudio != null)
            //  simpleAudio.MasterVolume = (float) e.NewValue;
        }

        private void SoundSlider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //manualSoundVolume = true;
        }

        private void SoundSlider_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //manualSoundVolume = false;
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
                        if (iVKGetAudio != null)
                        {
                            iVKGetAudio.UnShuffleList();
                        }
                        break;

                    case "Shuffle":
                        animateFontIcon.ChangeFontIconWithAnimation("\uE8B1");
                        if (iVKGetAudio != null)
                        {

                            iVKGetAudio.setShuffle();
                        }
                        break;

                    case "RepeatAll":
                        animateFontIcon.ChangeFontIconWithAnimation("\uE8EE");
                        if (iVKGetAudio != null)
                        {

                            iVKGetAudio.UnShuffleList();
                        }
                        break;


                    default:
                        break;
                }

            }
            catch (Exception ex)
            { 
            
            }
        }

        private void repeatBTN_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                switch (SettingsTable.GetSetting("playNext").settingValue)
                {
                    case "RepeatOne":
                        SettingsTable.SetSetting("playNext", "RepeatAll");

                        break;

                    case "Shuffle":
                        SettingsTable.SetSetting("playNext", "RepeatOne");

                        break;

                    case "RepeatAll":
                        SettingsTable.SetSetting("playNext", "Shuffle");

                        break;


                    default:
                        break;
                }
                setButtonPlayNext();
            }
            catch { 
                
            }

        }
        AnimationsChangeFontIcon statusAnimate;


        public void setStatusIcon()
        {

            var share = SettingsTable.GetSetting("shareFriend");
            if (share == null)
            {
                share = new SettingsTable();
                share.settingName = "shareFriend";
                share.settingValue = "false";
                DatabaseHandler.getConnect().Insert(share);
            }
            var a = bool.Parse(share.settingValue);
            if (a)
            {
                statusAnimate.ChangeFontIconWithAnimation("\uE701");
            }
            else
            {
                statusAnimate.ChangeFontIconWithAnimation("\uEB5E");
            }


        }

        private void TranslatetoStatus_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var share = SettingsTable.GetSetting("shareFriend");
            if (share.settingValue == "false")
            {
                SettingsTable.SetSetting("shareFriend", "true");
                if (iVKGetAudio != null)
                    {
                    iVKGetAudio.shareToVK();
                }
                }
            else
            {
                SettingsTable.SetSetting("shareFriend", "false");
            }





            setStatusIcon();
        }

        private void NextBTN_Tapped(object sender, TappedRoutedEventArgs e)
        {

            PlayNextTrack();

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

        }
        // --- Исправление: добавляю обработчик изменения размера ---
        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AnimateRedRectangle();
            this.DispatcherQueue.TryEnqueue(() =>
            {
                sliderTrackGridUP.Width = RootGrid.ActualWidth;
            });
        }

        // --- Исправление: проверяю, что AnimateRedRectangle объявлен ---
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
            if (percent < 0) percent = 0;
            if (percent > 1) percent = 1;
            RedFillPercent = percent;
        }

        private static AudioEqualizer _equalizer = null;
        public static AudioEqualizer Equalizer
        {
            get { return _equalizer; }
            set
            {
                _equalizer = value;
                PlayTrack(position: mediaPlayer.Position);
            }
        }

        public static WeakEventManager TrackDataThisChanged = new WeakEventManager();

        private static ExtendedAudio _trackDataThis;
        public static async Task<ExtendedAudio> _TrackDataThisGet(bool prinud = false)
        {
            if (iVKGetAudio != null)
                if (iVKGetAudio.countTracks != 0)
                {
                    return await iVKGetAudio.GetTrackPlay(prinud);
                }
            return _trackDataThis;
        }
        public ExtendedAudio TrackDataThis
        {
            get { return _TrackDataThisGet().Result; }
        }

        private long _sliderPositionMs;
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

        private void Page_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var storyboard = (Storyboard)this.Resources["SliderTrackMoveUp"];
            storyboard.Begin();
        }

        private void Page_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var storyboard = (Storyboard)this.Resources["SliderTrackMoveDown"];
            storyboard.Begin();
        }
    }
}
