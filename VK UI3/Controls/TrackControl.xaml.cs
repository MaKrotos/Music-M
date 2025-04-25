using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using VK_UI3.Controllers;
using VK_UI3.Converters;
using VK_UI3.DB;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VkNet.Model.Attachments;
using Windows.UI.Core;
using static VK_UI3.Helpers.ExtendedAudio;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class TrackControl : UserControl, INotifyPropertyChanged
    {

        public TrackControl()
        {
            this.InitializeComponent();

     
            Loaded += TrackControl_Loaded;
            Unloaded += TrackControl_Unloaded;



            this.Loading += TrackControl_Loading;
            this.DataContextChanged += TrackControl_DataContextChanged;
            if (changeImage == null)
                changeImage = new AnimationsChangeImage(ImageThumb, DispatcherQueue);
            changeIconPlayBTN = new AnimationsChangeIcon(PlayBTN);
        }

       

        private void TrackControl_Unloaded(object sender, RoutedEventArgs e)
        {
            addedHandler = false;
            if (dataTrack != null)
            {
                AudioPlayer.AudioPlayedChangeEvent -= UserAudio_AudioPlayedChangeEvent;
            }
            //   this.DataContextChanged -= TrackControl_DataContextChanged;
            //   Loaded -= TrackControl_Loaded;
            //   Unloaded -= TrackControl_Unloaded;

        }
        string? berImageLink = "";

        private void TrackControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if ((DataContext as ExtendedAudio) != null)
            {
                if (dataTrack != null)
                {
                    dataTrack.trackSelectChanged -= trackSelectChanged;
                }
               
               // if (dataTrack != null)
                  //  dataTrack.iVKGetAudio.AudioPlayedChangeEvent -= UserAudio_AudioPlayedChangeEvent;

                var track = (DataContext as ExtendedAudio).audio;
                if (track == null)
                    return;



                dataTrack = (DataContext as ExtendedAudio);
                string? newLink = "";
                if (dataTrack.audio.Thumb != null)
                {
                    newLink =
                       dataTrack.audio.Thumb.Photo270 ??
                       dataTrack.audio.Thumb.Photo300 ??
                       dataTrack.audio.Thumb.Photo600 ??
                       dataTrack.audio.Thumb.Photo34 ??
                       "";
                }

                dataTrack.trackSelectChanged += trackSelectChanged;

                if (dataTrack.audio.Album != null && dataTrack.audio.Album.Thumb != null && newLink.Equals(""))
                    newLink = 

                    dataTrack.audio.Album.Thumb.Photo270 ??
                    dataTrack.audio.Album.Thumb.Photo300 ??
                    dataTrack.audio.Album.Thumb.Photo600 ??
                    dataTrack.audio.Album.Thumb.Photo34 ??
                    null;
                if (berImageLink != newLink)
                {
                    ImageThumb.Opacity = 0;
                }
                berImageLink = newLink;
                updateUI();

                FlyOutControl.dataTrack = DataContext as ExtendedAudio;
            }
        }

        private void trackSelectChanged(object sender, EventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if ((e as SelectedChange).selected)
                {
                    FadeOutStoryboardSelectedGrid.Pause();
                    FadeInStoryboardSelectedGrid.Begin();
                }
                else
                {
                    FadeInStoryboardSelectedGrid.Pause();
                    FadeOutStoryboardSelectedGrid.Begin();
                }
  


            });
           
        }

        private void updateUI()
        {
       
            bool isOwner = dataTrack.audio.OwnerId == AccountsDB.activeAccount.id;
            if (dataTrack.audio.Url == null)
            {
                this.Opacity = 0.3; 
            }
            else
            {
                this.Opacity = 1;
            }

            if (dataTrack.iVKGetAudio.AudioIsSelect(dataTrack))
            {
                SelectedGrid.Opacity = 0.3;
            }
            else
            {
                SelectedGrid.Opacity = 0;
            }

            if (!(dataTrack.audio.AudioChartInfo is null))
            {
                switch (dataTrack.audio.AudioChartInfo.State)
                {
                    case ChartState.Crown:
                        this.chartState.Text = "👑";
                        break;
                    case ChartState.New_Release:
                        this.chartState.Text = "🆕";
                        break;
                  
                    case ChartState.Moved_up:
                        this.chartState.Text = "👆";
                        break;
                    case ChartState.Moved_down:
                        this.chartState.Text = "👇";
                        break;
                    default:
                        this.chartState.Text = "";
                        break;
                }

            }
            else
            {
                this.chartState.Text = "";
            }



            Title.Text = dataTrack.audio.Title;
            if (string.IsNullOrEmpty(dataTrack.audio.Subtitle))
            {
                colDefs.Width = new GridLength(0);
            }
            else
            {
                colDefs.Width = new GridLength(0.8, GridUnitType.Star);
            }
            Subtitle.Text = dataTrack.audio.Subtitle;
            Artists.Text = dataTrack.audio.Artist;



            if (dataTrack.audio.Album != null && dataTrack.audio.Album.Thumb != null)
            {

                if (berImageLink == null || berImageLink == "")
                {
                    ImageThumbGrid.Opacity = 0;
                }
                else
                {
                    ImageThumbGrid.Opacity = 1;

                    changeImage.ChangeImageWithAnimation(
                        berImageLink, true
                     );
                }
            }
            else
            {
                ImageThumbGrid.Opacity = 0;
            }



            if ((dataTrack.audio.MainArtists == null) || (!dataTrack.audio.MainArtists.Any()))
            {
                Artists.Text = dataTrack.audio.Artist;
            }
            else
            {

            }
            // Создание экземпляра конвертера
            SecondsToTimeConverter converter = new SecondsToTimeConverter();

            // Преобразование секунд в строку времени
            object timeString = converter.Convert(dataTrack.audio.Duration, null, null, null);
            Console.WriteLine(timeString);
            Time.Text = (string)timeString;

            try
            {
                Symbol symbol = dataTrack.PlayThis ? Symbol.Pause : Symbol.Play;
                ChangeSymbolIcon(symbol);
                HandleAnimation(dataTrack.PlayThis);
            }
            catch (Exception ex)
            {

            }

            if (dataTrack != null && !addedHandler)
            {
                AudioPlayer.AudioPlayedChangeEvent += UserAudio_AudioPlayedChangeEvent;
                addedHandler = true;
            }
        }



        protected void OnPropertyChanged(string propertyName)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;




        private void TrackControl_Loading(FrameworkElement sender, object args)
        {


        }

        ExtendedAudio dataTrack = null;
        bool addedHandler = false;


        AnimationsChangeImage changeImage = null;
        private void TrackControl_Loaded(object sender, RoutedEventArgs e)
        {
           
        }
       
        private void UserAudio_AudioPlayedChangeEvent(object sender, EventArgs e)
        {
            try
            {
                var a = dataTrack.audio.AccessKey == (sender as ExtendedAudio).audio.AccessKey;
                Symbol symbol =  a ? Symbol.Pause : Symbol.Play;
                this.DispatcherQueue.TryEnqueue(async() =>
                {
                    ChangeSymbolIcon(symbol);
                    HandleAnimation(a);
                });
            }
            catch (Exception ex)
            {
            }
        }


        private void ChangeSymbolIcon(Symbol symbol)
        {
            if (GridPlayIcon.Opacity != 0)
            {
                changeIconPlayBTN.ChangeSymbolIconWithAnimation(symbol);
            }
            else
            {
                PlayBTN.Symbol = symbol;
            }
        }

        private void HandleAnimation(bool playThis)
        {
            if (playThis)
            {
                FadeOutAnimationGridPlayIcon.Pause();
                FadeInAnimationGridPlayIcon.Begin();
            }
            else if (!entered)
            {
                FadeInAnimationGridPlayIcon.Pause();
                FadeOutAnimationGridPlayIcon.Begin();
            }
        }

      
        AnimationsChangeIcon changeIconPlayBTN = null;
        bool entered = false;
        private void UCcontrol_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //FadeInAnimation.Begin();
            if (dataTrack == null) return;
            if (dataTrack.audio.Url == null) return;
            entered = true;

            var pointerModifiers = e.KeyModifiers;

            if (
                pointerModifiers.HasFlag(Windows.System.VirtualKeyModifiers.Control)
             || pointerModifiers.HasFlag(Windows.System.VirtualKeyModifiers.Shift)
                )
            {
                FadeOutStoryboardBorderSelectedGrid.Pause();
                FadeInStoryboardBorderSelectedGrid.Begin();
            }

            Symbol symbol = dataTrack.PlayThis ? Symbol.Pause : Symbol.Play;
            if (GridPlayIcon.Opacity != 0)
            {
                changeIconPlayBTN.ChangeSymbolIconWithAnimation(symbol);
            }
            else
            {
                PlayBTN.Symbol = symbol;
            }

            FadeOutAnimationGridPlayIcon.Pause();
            FadeInAnimationGridPlayIcon.Begin();

      
        }



        private void UCcontrol_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            entered = false;
            var a = DataContext as ExtendedAudio;
            if (a == null) return;
            if (a.PlayThis) return;
            FadeInAnimationGridPlayIcon.Pause();
            FadeOutAnimationGridPlayIcon.Begin();

            FadeInStoryboardBorderSelectedGrid.Pause();
            FadeOutStoryboardBorderSelectedGrid.Begin();
        }

        private DateTime _pressStartTime;



        private void UCcontrol_Tapped(object sender, TappedRoutedEventArgs e)
        {

            // Проверяем состояние клавиш модификаторов
            var isCtrlPressed = InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
                                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            var isShiftPressed = InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift)
                                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);


            if (isCtrlPressed || isShiftPressed)
            {
                dataTrack.iVKGetAudio.changeSelect(dataTrack, isShiftPressed);
                return;
            }

            if (dataTrack.PlayThis && AudioPlayer.mediaPlayer.CurrentState != Windows.Media.Playback.MediaPlayerState.Paused)
            {
                AudioPlayer.mediaPlayer.Pause();
                return;
            }

            dataTrack.iVKGetAudio.playTrack(dataTrack.NumberInList);


        }

    }
}