using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using Windows.Graphics.Display;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Controls
{
    public class VidArgs
    {
        public TimeSpan position;
        public MediaPlayerState playing;
    }

    public sealed partial class VideoView : UserControl
    {
        public Dictionary<string, string> Quality;
        public string linkBrowser = null;
        public bool enableFullScreen { get; set; } = true;
        public VideoView(Dictionary<string, string> quality, string linkBrowser = null)
        {
            this.InitializeComponent();
            Quality = quality;
            this.linkBrowser = linkBrowser;


            this.KeyDown += VideoView_KeyDown;
        }

        private void VideoView_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                CloseButtonClicked?.Invoke(this, new VidArgs() {
                
                    position = VidePl.MediaPlayer.Position,
                    playing = VidePl.MediaPlayer.CurrentState
                });
            }
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            CloseButtonClicked?.Invoke(this, new VidArgs()
            {

                position = VidePl.MediaPlayer.Position,
                playing = VidePl.MediaPlayer.CurrentState
            });
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (enableFullScreen)
            {
                FullScreen.Visibility = Visibility.Visible;
                FadeOutAnimationFullScreen.Pause();
                FadeInAnimationFullScreen.Begin();
            }
            if (linkBrowser != null)
            {
                openInBrowser.Visibility = Visibility.Visible;
                FadeOutAnimationopenInBrowser.Pause();
                FadeInAnimationopenInBrowser.Begin();
            }
            if (CloseButtonClicked != null)
            {
                CloseBTN.Visibility = Visibility.Visible;
                FadeOutAnimationCloseBTN.Pause();
                FadeInAnimationCloseBTN.Begin();
            }


            qualityBox.Visibility = Visibility.Visible;
            FadeOutAnimationqualityBox.Pause();
            FadeInAnimationqualityBox.Begin();


        }
        

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (enableFullScreen)
            {

                FadeInAnimationFullScreen.Pause();
                FadeOutAnimationFullScreen.Begin();
            }
            if (linkBrowser != null)
            {
                FadeInAnimationopenInBrowser.Pause();
                FadeOutAnimationopenInBrowser.Begin();
            }

            if (!qualityBox.IsDropDownOpen)
            {
                FadeInAnimationqualityBox.Pause();
                FadeOutAnimationqualityBox.Begin();
            }

            FadeInAnimationCloseBTN.Pause();
            FadeOutAnimationCloseBTN.Begin();
        }

        private void FadeOutAnimationopenInBrowser_Completed(object sender, object e)
        {
            openInBrowser.Visibility = Visibility.Collapsed;
        }

        private void FadeOutAnimationqualityBox_Completed(object sender, object e)
        {
            qualityBox.Visibility = Visibility.Collapsed;
        }

        private void FadeOutAnimationFullScreen_Completed(object sender, object e)
        {
            FullScreen.Visibility = Visibility.Visible;
        }

        private void qualityBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Сохраняем текущую позицию
            TimeSpan currentPosition = VidePl.MediaPlayer.PlaybackSession.Position;

            // Получаем выбранное качество
            string selectedQuality = qualityBox.SelectedItem.ToString();

            // Получаем ссылку на видео соответствующего качества
            string videoUrl = Quality[selectedQuality];

            // Устанавливаем источник для MediaPlayerElement
            VidePl.Source = MediaSource.CreateFromUri(new Uri(videoUrl));

            // Восстанавливаем позицию
            VidePl.MediaPlayer.PlaybackSession.Position = currentPosition;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Заполняем ComboBox элементами из словаря
            foreach (var item in Quality)
            {
                qualityBox.Items.Add(item.Key);
            }

            // Устанавливаем выбранный элемент ComboBox на среднее качество
            if (qualityBox.Items.Count > 0)
            {
                int middleIndex = qualityBox.Items.Count / 2;
                qualityBox.SelectedIndex = middleIndex;
            }
        }

        private bool wasPlaying; 

       public MediaPlayerElement mediaPlayerElement { get { return VidePl; } }
        Window window;


        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            // Сохраняем состояние воспроизведения
            wasPlaying = VidePl.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;

            VidePl.MediaPlayer.Pause();

            if (window != null)
            {
                window.Activate();
                return;
            }

            // Создаем новое окно
            var newWindow = new Window();

            // Устанавливаем окно на весь экран
            newWindow.ExtendsContentIntoTitleBar = true;
            newWindow.SetTitleBar(null);
            newWindow.AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);

            var vidv = new VideoView(Quality);

            void Vidv_CloseButtonClicked(object sender, VidArgs e)
            {
            

                switch (e.playing)
                {
                    case MediaPlayerState.Closed:
                        VidePl.MediaPlayer.Pause();
                        break;
                    case MediaPlayerState.Opening:
                        VidePl.MediaPlayer.Pause();

                        break;
                    case MediaPlayerState.Buffering:
                        VidePl.MediaPlayer.Play();

                        break;
                    case MediaPlayerState.Playing:
                        VidePl.MediaPlayer.Play();

                        break;
                    case MediaPlayerState.Paused:
                        VidePl.MediaPlayer.Pause();

                        break;
                    case MediaPlayerState.Stopped:
                        VidePl.MediaPlayer.Pause();

                        break;
                    default:
                        break;
                }

                // Восстанавливаем состояние воспроизведения
                if (wasPlaying)
                {
                    VidePl.MediaPlayer.Play();
                }
                vidv.CloseButtonClicked -= Vidv_CloseButtonClicked;
                newWindow.Close();
                window = null;
                TimeSpan currentPosition = VidePl.MediaPlayer.PlaybackSession.Position;
                TimeSpan newPosition = e.position;
                VidePl.MediaPlayer.PlaybackSession.Position = newPosition;

                
            }

            vidv.CloseButtonClicked += Vidv_CloseButtonClicked;
            vidv.enableFullScreen = false;
            newWindow.Content = vidv;
            vidv.mediaPlayerElement.MediaPlayer.Pause();
            vidv.mediaPlayerElement.Source = null;
            // Открываем окно
            newWindow.Activate();
        }


    

        private void NewWindow_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Проверяем, является ли нажатая клавиша Esc
            if (e.Key == VirtualKey.Escape)
            {
                CloseButtonClicked?.Invoke(this, new VidArgs() { position = VidePl.MediaPlayer.Position, playing = VidePl.MediaPlayer.CurrentState });
            }
        }



        private async void openInBrowser_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(linkBrowser));
        }

        private void CloseBTN_Click(object sender, RoutedEventArgs e)
        {
          
            CloseButtonClicked?.Invoke(this, new VidArgs() {
                
            });
        }
        public event EventHandler<VidArgs> CloseButtonClicked;
        private void FadeOutAnimationCloseBTN_Completed(object sender, object e)
        {
            CloseBTN.Visibility = Visibility.Collapsed;
        }
    }
}
