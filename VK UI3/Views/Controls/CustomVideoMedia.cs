
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
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
