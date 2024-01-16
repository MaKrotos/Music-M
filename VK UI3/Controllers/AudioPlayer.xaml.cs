

//using CSCore.CoreAudioAPI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using VK_UI3.DB;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Interfaces;
using VK_UI3.Views;
using VK_UI3.VKs;
using VkNet.Model;
using VkNet.Model.Attachments;
using vkPosterBot.DB;
using Windows.Media.Playback;
using Windows.Storage.Streams;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controllers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    public sealed partial class AudioPlayer : Microsoft.UI.Xaml.Controls.Page, INotifyPropertyChanged
    /// </summary>
    {

        private static Windows.Media.Playback.MediaPlayer mediaPlayer = new MediaPlayer();

        public static event EventHandler oniVKUpdate; // Событие OnDeviceAttached
        public static void NotifyoniVKUpdate()
        {

            oniVKUpdate?.Invoke(null, EventArgs.Empty);
           
        
        }

        public MediaPlayer MediaPlayer
        {
            get { return mediaPlayer; }
            set { mediaPlayer = value; }
        }



        private int _trackPosition;
        public int TrackPosition
        {
            get { return _trackPosition; }
            set
            {
                if (_trackPosition != value)
                {
                    _trackPosition = value;
                    OnPropertyChanged(nameof(TrackPosition));
                }
            }
        }



        private int _trackPosition2;


        public static event EventHandler TrackDataThisChanged;


        public static Audio _TrackDataThis
        {
            get {
                if (iVKGetAudio != null)
                    if (iVKGetAudio.countTracks != 0) return iVKGetAudio.GetTrackPlay().Audio;
                return _trackDataThis; }
            set
            {
                if (_trackDataThis != value)
                {
                    _trackDataThis = value;
                    TrackDataThisChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        private static Audio _trackDataThis;
        public Audio TrackDataThis
        {
            get { return _TrackDataThis; }
        }

        private int _trackDuration = 0;
        public int TrackDuration
        {
            get { return _trackDuration; }
            set
            {
                if (_trackDuration != value)
                {
                    _trackDuration = value;
                    OnPropertyChanged(nameof(TrackDuration));
                }
            }
        }


        public string Thumbnail
        {
            get
            {
                if (TrackDataThis == null) return "null";
                if (TrackDataThis.Album == null) return "null";
                return TrackDataThis.Album.Thumb.Photo600
                     ?? TrackDataThis.Album.Thumb.Photo300
                     ?? TrackDataThis.Album.Thumb.Photo270
                     ?? TrackDataThis.Album.Thumb.Photo68
                     ?? TrackDataThis.Album.Thumb.Photo34
                     ?? "null";
            }

        }




        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });

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


            changeIconPlayBTN = new AnimationsChangeIcon(this.PlayBTN);
            animateFontIcon = new AnimationsChangeFontIcon(this.repeatBTNIcon);
            changeImage = new AnimationsChangeImage(this.ImageThumb, DispatcherQueue);
            changeText = new AnimationsChangeText(ArtistTextBlock, this.DispatcherQueue);
            changeText2 = new AnimationsChangeText(TitleTextBlock, this.DispatcherQueue);
            statusAnimate = new AnimationsChangeFontIcon(StatusBTNIcon);
            setStatusIcon();

            oniVKUpdate += AudioPlayer_oniVKUpdate;

            this.Loaded += AudioPlayer_Loaded;






            TrackDuration = 0;
            TrackPosition = 0;

            TrackDataThisChanged += AudioPlayer_PropertyChanged;

            // Привязка к событию MediaOpened
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;

            // Привязка к событию MediaEnded
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            // Привязка к событию MediaFailed
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;

            // Привязка к событию CurrentStateChanged
            mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            mediaPlayer.AutoPlay = true;

            // Привязка к событию SourceChanged
            mediaPlayer.SourceChanged += MediaPlayer_SourceChanged;
            mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
            mediaPlayer.PlaybackSession.BufferedRangesChanged += PlaybackSession_BufferedRangesChanged;


            mediaPlayer.CommandManager.NextReceived += CommandManager_NextReceived;
            mediaPlayer.CommandManager.PreviousReceived += CommandManager_PreviousReceived;
            mediaPlayer.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            mediaPlayer.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;

        }
        double actualHeight = 0;

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

        void DisableAllChildren(DependencyObject parent, bool enable = false)
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
        }


        private void CommandManager_PreviousReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPreviousReceivedEventArgs args)
        {
            PlayPreviousTrack();
        }

        private void CommandManager_NextReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerNextReceivedEventArgs args)
        {
            PlayNextTrack();
        }



        private void PlaybackSession_BufferedRangesChanged(MediaPlaybackSession sender, object args)
        {
            var a = (int) Math.Round(sender.NaturalDuration.TotalSeconds);
            var b = TrackDataThis.Duration;

            if (Math.Abs(a - b) > 1)
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
            // Код для выполнения при завершении воспроизведения медиафайла


      


            
                switch (SettingsTable.GetSetting("playNext").settingValue)
                {
                    case "RepeatOne":
                        PlayTrack();
                        break;

                    case "Shuffle":
                        PlayNextTrack();

                        break;

                    case "RepeatAll":

                        PlayNextTrack();
                        break;


                    default:
                        break;
                }

          
        }
        private void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            if (!isManualChange)
            {
                TrackPosition = Convert.ToInt32(sender.Position.TotalSeconds);

            }
        }

        private void MediaPlayer_MediaOpened(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            // Код для выполнения при открытии медиафайла
            TrackDuration = Convert.ToInt32(sender.NaturalDuration.TotalSeconds);
        }
        private void MediaPlayer_MediaFailed(Windows.Media.Playback.MediaPlayer sender, Windows.Media.Playback.MediaPlayerFailedEventArgs args)
        {
            // Код для выполнения при ошибке воспроизведения медиафайла
        }
        AnimationsChangeIcon changeIconPlayBTN = null;
        private void MediaPlayer_CurrentStateChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            // Код для выполнения при изменении состояния медиаплеера
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                switch (sender.CurrentState)
                {
                    case (MediaPlayerState)Windows.Media.Playback.MediaPlaybackState.Playing:

                        changeIconPlayBTN.ChangeSymbolIconWithAnimation(Symbol.Pause);
                        break;
                    case (MediaPlayerState)Windows.Media.Playback.MediaPlaybackState.Paused:
                        changeIconPlayBTN.ChangeSymbolIconWithAnimation(Symbol.Play);

                        break;
                    case (MediaPlayerState)Windows.Media.Playback.MediaPlaybackState.Buffering:

                        break;
                    case MediaPlayerState.Closed:
                        PlayTrack(iVKGetAudio.currentTrack);
                        break;
                    default:
                        changeIconPlayBTN.ChangeSymbolIconWithAnimation(Symbol.Sync);
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
            //Код для выполнения при изменении источника медиаплеера

            var source = sender.Source as Windows.Media.Playback.MediaPlaybackItem;

            TrackDuration = Convert.ToInt32(sender.NaturalDuration.TotalSeconds);
            OnPropertyChanged(nameof(TrackDuration));
            OnPropertyChanged(nameof(TrackPosition));
            OnPropertyChanged(nameof(TrackDataThis));

            changeImage.ChangeImageWithAnimation(Thumbnail);
            changeText.ChangeTextWithAnimation(TrackDataThis.Artist);
            changeText2.ChangeTextWithAnimation(TrackDataThis.Title);

           


            if (bool.Parse(SettingsTable.GetSetting("shareFriend").settingValue))
                iVKGetAudio.shareToVK();

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
                mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);
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
        }





        private void Button_Play_Tapped(object sender, TappedRoutedEventArgs e)
        {

            switch (mediaPlayer.CurrentState)
            {
                case (MediaPlayerState)Windows.Media.Playback.MediaPlaybackState.Playing:

                    mediaPlayer.Pause();
                    break;
                case (MediaPlayerState)Windows.Media.Playback.MediaPlaybackState.Paused:
                    mediaPlayer.Play();

                    break;
                case (MediaPlayerState)Windows.Media.Playback.MediaPlaybackState.Buffering:

                    break;


                default:
                    break;
            }

        }
       
        public static IVKGetAudio iVKGetAudio = null;
    
     


        internal static void PlayNextTrack()
        {
            // Проверяем, не является ли текущий трек последним в списке

            iVKGetAudio.getNextTrackForPlay();
            PlayTrack();
       
        }

        internal static void PlayPreviousTrack()
        {

            iVKGetAudio.getPreviusTrackForPlay();
            PlayTrack();
            
        }

        private async static void PlayTrack(long? v = null)
        {
            if (v != null) iVKGetAudio.currentTrack = (long)v;

            iVKGetAudio.ChangePlayAudio();


            var mediaSource = Windows.Media.Core.MediaSource.CreateFromUri(new Uri(_TrackDataThis.Url.ToString()));
            var mediaPlaybackItem = new Windows.Media.Playback.MediaPlaybackItem(mediaSource);

          


            MediaItemDisplayProperties props = mediaPlaybackItem.GetDisplayProperties();
            props.Type = Windows.Media.MediaPlaybackType.Music;
            props.MusicProperties.Title = _TrackDataThis.Title;
            props.MusicProperties.AlbumArtist = _TrackDataThis.Artist;


            if (_TrackDataThis.Album != null)
            {
                RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromUri(new Uri(
                    _TrackDataThis.Album.Thumb.Photo600 ??
                    _TrackDataThis.Album.Thumb.Photo270 ??
                    _TrackDataThis.Album.Thumb.Photo300 
                
     
                    ));

                props.Thumbnail = imageStreamRef;
                mediaPlaybackItem.ApplyDisplayProperties(props);

                props.Thumbnail = imageStreamRef;

            }
            else props.Thumbnail = null;
            mediaPlaybackItem.ApplyDisplayProperties(props);

            mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
            mediaPlayer.Pause();
         
            // mediaPlayer.Source = null;
            mediaPlayer.Volume = 1;
            mediaPlayer.Source = mediaPlaybackItem;
            mediaPlayer.Play();
           
        }

       

        private void PreviousBTN_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PlayPreviousTrack();
        }

        private void goToPlayList_BTN(object sender, TappedRoutedEventArgs e)
        {
            var navigationInfo = new NavigationInfo { SourcePageType = this };
            MainWindow.contentFrame.Navigate(typeof(PlayList), navigationInfo, new DrillInNavigationTransitionInfo());
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
            manualSoundVolume = true;
        }

        private void SoundSlider_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            manualSoundVolume = false;
        }

        private void setButtonPlayNext() {
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

        private void repeatBTN_Tapped(object sender, TappedRoutedEventArgs e)
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

        internal static void PlayList(IVKGetAudio userAudio)
        {
            iVKGetAudio = userAudio;
            AudioPlayer.PlayTrack();
            NotifyoniVKUpdate();
        }
    }
}
