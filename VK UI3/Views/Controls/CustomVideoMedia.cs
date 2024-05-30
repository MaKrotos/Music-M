using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace VK_UI3.Views.Controls
{
    internal class CustomVideoMedia : MediaPlayerElement
    {
        public Storyboard _storyboard;
  
 
        public Storyboard _storyboard2;
        public CustomVideoMedia() : base()
        {

    

           
            this.Loaded += CustomVideoMedia_Loaded;
        }

        public void setSource(string source) 
        {


            this.MediaPlayer.Source = MediaSource.CreateFromUri(new Uri(source));
            this.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            this.MediaPlayer.Play();
        }

        private void CustomVideoMedia_Loaded(object sender, RoutedEventArgs e)
        {
        
            this.MediaPlayer.IsMuted = true; // выключаем звук
            _storyboard = new Storyboard();
            _storyboard.Children.Add(Showanimation);
            Storyboard.SetTarget(Showanimation, this);
            Storyboard.SetTargetProperty(Showanimation, "Opacity");

            _storyboard2 = new Storyboard();
            _storyboard2.Children.Add(HideAnimation);
            Storyboard.SetTarget(HideAnimation, this);
            Storyboard.SetTargetProperty(HideAnimation, "Opacity");


            _storyboard2.Completed += _storyboard2_Completed;
            this.MediaPlayer.AutoPlay = false; // включаем автозапуск
            this.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            this.MediaPlayer.Play(); 
        }

        private void _storyboard2_Completed(object sender, object e)
        {
            if (Opacity == 0)
            {
                HIDED.Invoke(this, EventArgs.Empty);
            }
        }

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


        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
          
                _storyboard.Begin();
          
        }
        public EventHandler HIDED;
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
    }

}
