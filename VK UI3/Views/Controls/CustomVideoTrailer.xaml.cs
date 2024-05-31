using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Controls
{

    public sealed partial class CustomVideoTrailer : UserControl
    {
        public Storyboard _storyboard;
        public Storyboard _storyboard2;
        public CustomVideoTrailer()
        {
            this.InitializeComponent();
            this.Loaded += CustomVideoMedia_Loaded;
        }


        public void setSource(string source)
        {
            VideoSources.MediaPlayer.Source = MediaSource.CreateFromUri(new Uri(source));
            VideoSources.MediaPlayer.Play();
        }

        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                _storyboard.Begin();
            });
        }
       
        private void CustomVideoMedia_Loaded(object sender, RoutedEventArgs e)
        {


            DoubleAnimation Showanimation = new DoubleAnimation
            {
                To = 1.0,
                Duration = new Duration(TimeSpan.FromSeconds(1))
            };

            DoubleAnimation HideAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromSeconds(1))
            };


            VideoSources.MediaPlayer.IsMuted = true; // выключаем звук
            _storyboard = new Storyboard();
            _storyboard.Children.Add(Showanimation);
            Storyboard.SetTarget(Showanimation, VideoSources);
            Storyboard.SetTargetProperty(Showanimation, "Opacity");

            _storyboard2 = new Storyboard();
            _storyboard2.Children.Add(HideAnimation);
            Storyboard.SetTarget(HideAnimation, VideoSources);
            Storyboard.SetTargetProperty(HideAnimation, "Opacity");


            _storyboard2.Completed += _storyboard2_Completed;
            VideoSources.MediaPlayer.AutoPlay = false; // включаем автозапуск
            VideoSources.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }
        public void hide()
        {
            _storyboard.Pause();
            _storyboard2.Begin();
        }

        public void show()
        {
            _storyboard2.Pause();
            _storyboard.Begin();

        }
        public EventHandler HIDED;

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                VideoSources.MediaPlayer.Play();
            });
        }

        private void _storyboard2_Completed(object sender, object e)
        {
            if (Opacity == 0)
            {
                HIDED.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
