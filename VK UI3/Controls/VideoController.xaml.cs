using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using MusicX.Core.Models;
using MusicX.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VK_UI3.Views.Controls;
using VK_UI3.Views.ModalsPages;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Playlists;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class VideoController : UserControl
    {
        Helpers.Animations.AnimationsChangeImage AnimationsChangeImage;
        public VideoController()
        {
            this.InitializeComponent();
            AnimationsChangeImage = new AnimationsChangeImage(imageVideo, this.DispatcherQueue);
          

            this.Unloaded += PlaylistControl_Unloaded;


            DataContextChanged += VideoController_DataContextChanged;

            HideAnimationMediaP.Completed += HideAnimationMediaP_Completed;


        }

        private void HideAnimationMediaP_Completed(object sender, object e)
        {
            if (ShowVidGid.Opacity == 1)
            { 
                if (VideoSources.MediaPlayer != null)
                {
                    VideoSources.MediaPlayer.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
                }
                VideoSources.Source = null;
            }
        }

        public bool setCOlorTheme { get; set; } = false;
        MusicX.Core.Models.Video video = null;
        private void VideoController_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            if (DataContext is MusicX.Core.Models.Video vid)
            {
                if (video == vid) return;
                video = vid;
                imageVideo.Source = null;
                AnimationsChangeImage.ChangeImageWithAnimation(video.Image.LastOrDefault().Url);


                HideAnimationMediaP.Pause();
                ShowAnimationMediaP.Pause();
                ShowVidGid.Opacity = 1;
                VideoSources.Source = null;


                MainText.Text = video.Title;


                if (video.MainArtists != null && video.MainArtists.Count != 0)
                {

                    string artists = string.Join(", ", video.MainArtists.Select(g => g.Name));
                    SecondText.Text = artists;
                    SecondText.Visibility = Visibility.Visible;
                }
                else
                {
                    SecondText.Visibility = Visibility.Collapsed;
                }
                string textG = null;
                if (video.Genres != null && video.Genres.Count != 0)
                {
                    textG = video.Genres[0].Name;
                }

                DateTime date = DateTime.FromFileTimeUtc(video.ReleaseDate);
                if (textG == null)
                {

                    textG = date.Year.ToString();

                }
                else
                {
                    textG += ", " + date.Year.ToString();
                }



            }
        }

      

        private void PlaylistControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= PlaylistControl_Unloaded;
        }





        private async void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ContentDialog dialog = new CustomDialog();

            dialog.Transitions = new TransitionCollection
                {
                    new PopupThemeTransition()
                };

            dialog.XamlRoot = this.XamlRoot;

            var videoView = new VideoView(video.files.Quality, video.Player);
            videoView.MaxHeight = 300;
            dialog.Content = videoView;
            dialog.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

            EventHandler<VidArgs> closeButtonClickedHandler = null;
            closeButtonClickedHandler = (s, e) =>
            {
                if (dialog != null)
                {
                    dialog.Hide();
                    videoView.CloseButtonClicked -= closeButtonClickedHandler;
                    dialog = null;
                    videoView.mediaPlayerElement.MediaPlayer.Pause();
                    videoView.mediaPlayerElement.Source = null;
                }
            };

            videoView.CloseButtonClicked += closeButtonClickedHandler;

            dialog.ShowAsync();
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            show = true;
            FadeOutAnimationGridPlayIcon.Pause();
            FadeInAnimationGridPlayIcon.Begin();


            HideAnimation.Pause();
            ShowAnimation.Begin();

            if (video.trailer == null) return;
            var qa = video.trailer.Quality;
            if (qa.Count == 0) return;


        

            if (VideoSources.Source == null)
            {
                var qualityList = qa.Keys.Select(k => int.Parse(k.TrimEnd('p'))).OrderBy(q => q).ToList();
                var middleQuality = qualityList[qualityList.Count / 2];
                var middleQualityUrl = qa[middleQuality + "p"];
                VideoSources.Source = MediaSource.CreateFromUri(new Uri(middleQualityUrl));
          
                VideoSources.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                VideoSources.MediaPlayer.Play();
            }
            else
            {
                HideAnimationMediaP.Pause();
                ShowAnimationMediaP.Begin();
            }


        }

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                switch (VideoSources.MediaPlayer.CurrentState)
                {
                    case MediaPlayerState.Playing:
                        if (show)
                        {
                            this.DispatcherQueue.TryEnqueue(async () =>
                            {
                                HideAnimationMediaP.Pause();
                                ShowAnimationMediaP.Begin();
                            });
                        }
                        else
                        {
                            this.DispatcherQueue.TryEnqueue(async () =>
                            {
                                ShowAnimationMediaP.Pause();
                                HideAnimationMediaP.Begin();
                            });
                        }
                        break;
                    default:
                        this.DispatcherQueue.TryEnqueue(async () =>
                        {
                            ShowAnimationMediaP.Pause();
                            HideAnimationMediaP.Begin();
                        });
                        break;
                }
            });
        }

        bool show = false;

      



        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FadeInAnimationGridPlayIcon.Pause();
            FadeOutAnimationGridPlayIcon.Begin();
            ShowAnimation.Pause();
            HideAnimation.Begin();
            show = false;
            ShowAnimationMediaP.Pause();
            HideAnimationMediaP.Begin();

       
        }






    }
}
