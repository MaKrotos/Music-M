using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using VK_UI3.Controllers;
using VK_UI3.DB;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VK_UI3.Views.ModalsPages;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class VibeControl : UserControl
    {
        public VibeControl()
        {
            this.InitializeComponent();

            AnimationsChangeFontIcon = new AnimationsChangeFontIcon(PlayPause, this.DispatcherQueue);
            titleAnim = new AnimationsChangeText(Title, this.DispatcherQueue);
         
            this.Unloaded += PlaylistControl_Unloaded;
            this.Loaded += PlaylistControl_Loaded;

            DataContextChanged += RecommsPlaylist_DataContextChanged;


        }

        private void PlaylistControl_Loaded(object sender, RoutedEventArgs e)
        {
            FadeOutAnimationGrid.Completed += FadeOutAnimationGrid_Completed;
            AudioPlayer.oniVKUpdate += AudioPlayer_oniVKUpdate;
        }

        private void PlaylistControl_Unloaded(object sender, RoutedEventArgs e)
        {
            AudioPlayer.oniVKUpdate -=AudioPlayer_oniVKUpdate;

            this.Unloaded -= PlaylistControl_Unloaded;
            this.Loaded -= PlaylistControl_Loaded;
            FadeOutAnimationGrid.Completed -= FadeOutAnimationGrid_Completed;
            DataContextChanged -= RecommsPlaylist_DataContextChanged;

        }

        private void AudioPlayer_oniVKUpdate(object sender, EventArgs e)
        {
            updatePlayState();
        }
    
        private void updatePlayState(bool prin = false, bool pause = false)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                string icon = "\uE768"; // default icon

                if (prin)
                {
                    icon = pause ? "\uE768" : "\uE769";
                }
                else if (isThisPlayList_Now_Play && AudioPlayer.mediaPlayer.PlaybackSession.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Paused)
                {
                    icon = "\uE769";
                }

                if (!isThisPlayList_Now_Play)
                {
                    if (!entered)
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

                AnimationsChangeFontIcon.ChangeFontIconWithAnimation(icon);
            });
        }

        public bool isThisPlayList_Now_Play
        {
            get
            {
                if (_PlayList == null || AudioPlayer.iVKGetAudio == null)
                    return false;

                if (!(AudioPlayer.iVKGetAudio is PlayListVK))
                    return false;

                var playlist = (AudioPlayer.iVKGetAudio as PlayListVK).playlist;

                if (playlist == _PlayList)
                    return true;

                if (playlist.Id == _PlayList.Id && playlist.OwnerId == _PlayList.OwnerId && playlist.AccessKey == _PlayList.AccessKey)
                    return true;

                if (playlist.Original != null)
                {
                    if (playlist.Id == playlist.Original.PlaylistId && playlist.OwnerId == playlist.Original.OwnerId && playlist.AccessKey == playlist.Original.AccessKey)
                        return true;
                }

                return false;
            }
        }


        AnimationsChangeText titleAnim { get; set; }
        
        


        public static readonly DependencyProperty PlaylistItemsProperty =
         DependencyProperty.Register(
            "_PlaylistItems",
             typeof(ObservableRangeCollection<AudioPlaylist>),
             typeof(PlaylistControl),
             new PropertyMetadata(default(ObservableRangeCollection<AudioPlaylist>)));

        public DependencyProperty PlaylistItems => PlaylistItemsProperty;
        public ObservableRangeCollection<AudioPlaylist> _PlaylistItems
        {
            get { return (ObservableRangeCollection<AudioPlaylist>)GetValue(PlaylistItemsProperty); }
            set { SetValue(PlaylistItemsProperty, value); }
        }

     

       

       
        AnimationsChangeFontIcon AnimationsChangeFontIcon;
        AnimationsChangeImage animationsChangeImage;

        AudioPlaylist _PlayList { get; set; }
   

        private void RecommsPlaylist_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            _PlayList = (DataContext as AudioPlaylist);
           
            update();
        }

        private void FadeOutAnimationGrid_Completed(object sender, object e)
        {
            update();
        }

        public void update() {

            AnimationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
   
            titleAnim.ChangeTextWithAnimation(_PlayList.Title);
         
      

            if (_PlayList.Cover != null)
            {
                SmallHelpers.AddImagesToGrid(GridThumbs, _PlayList.Cover, this.DispatcherQueue);
            }
            else if (_PlayList.Thumbs != null)
            {
                int count = _PlayList.Thumbs.Count;
                int index = 0;
                List<string> list = new List<string>();
                foreach (var item in _PlayList.Thumbs)
                {
                    string photo = item.Photo600 ?? item.Photo1200 ?? item.Photo300 ?? item.Photo34 ?? item.Photo270 ?? item.Photo135 ?? item.Photo68;
                    list.Add(photo);
                    index++;
                }
                SmallHelpers.AddImagesToGrid(GridThumbs, list, this.DispatcherQueue);
            }
           

       
            updatePlayState();

            FadeInAnimationGrid.Begin();
        }

      

        bool entered;
        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            entered = true;
            FadeOutAnimationGridPlayIcon.Pause();
            FadeInAnimationGridPlayIcon.Begin();
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            entered = false;
            if (!isThisPlayList_Now_Play)
            {
                FadeInAnimationGridPlayIcon.Pause();
                FadeOutAnimationGridPlayIcon.Begin();
            }
        }

            private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(sender as UIElement).Properties.IsLeftButtonPressed)
            {
                if (iVKGetAudio == null)
                    MainView.OpenPlayList(_PlayList);
                else
                {
                    MainView.OpenPlayList(iVKGetAudio);
                }
            }
        }


        IVKGetAudio iVKGetAudio = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (isThisPlayList_Now_Play)
            {
                if (AudioPlayer.mediaPlayer.PlaybackSession.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Paused)
                {
                    AudioPlayer.mediaPlayer.Pause();
                    updatePlayState(true, true);
                }
                else
                {
                    AudioPlayer.mediaPlayer.Play();
                    updatePlayState(true, false);
                }

            }
            else
            {
                iVKGetAudio = new PlayListVK(_PlayList, this.DispatcherQueue);
                AnimationsChangeFontIcon.ChangeFontIconWithAnimation("\uE916");
                iVKGetAudio.PlayThis();
            }
        }



     
    }
}