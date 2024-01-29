using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VK_UI3.Helpers.Animations;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class TrackControl : UserControl
    {
        public TrackControl()
        {
            this.InitializeComponent();

            Loaded += TrackControl_Loaded; ;
        }
        AnimationsChangeImage changeImage;
        private void TrackControl_Loaded(object sender, RoutedEventArgs e)
        {
            var track = DataContext as Audio;
            if (track == null) 
                return;
            Title.Text = track.Title;
            Subtitle.Text = track.Subtitle;
            Artists.Text = track.Artist;
            changeImage = new AnimationsChangeImage(Cover, DispatcherQueue);
        }

        public void Title_PointerPressed(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        public void Title_PointerExited(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        public void Title_PointerEntered(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        public void RecommendedAudio_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        public void PlayNext_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        public void GoToArtist_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        public void Download_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        public void AddToQueue_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        public void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        public void AddRemove_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }
        public void AddArtistIgnore_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }
    }
}
