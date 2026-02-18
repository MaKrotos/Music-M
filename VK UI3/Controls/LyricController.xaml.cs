using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using MusicX.Core.Models;
using System;
using VK_UI3.Views.ModalsPages;
using Windows.Media.Playback;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Microsoft.UI;
using VK_UI3.Controllers;

namespace VK_UI3.Controls
{
    public sealed partial class LyricController : UserControl
    {
        private static readonly SolidColorBrush DefaultTextBrush = new SolidColorBrush(Colors.White);

        public LyricController()
        {
            this.InitializeComponent();
            this.DataContextChanged += DefaultControl_DataContextChanged;
            this.Unloaded += DefaultControl_Unloaded;
            this.Loaded += LyricController_Loaded;
        }

        private void LyricController_Loaded(object sender, RoutedEventArgs e)
        {
            LyricsPage.timerTickCangeCheck += LyricsPage_timerTickCangeCheck;
        }

        private void DefaultControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.DataContextChanged -= DefaultControl_DataContextChanged;
            this.Unloaded -= DefaultControl_Unloaded;
            LyricsPage.timerTickCangeCheck -= LyricsPage_timerTickCangeCheck;
            this.Loaded -= LyricController_Loaded;
        }

        private void LyricsPage_timerTickCangeCheck(object sender, System.EventArgs e)
        {
            if (IsLyricsTimestampAtPosition((e as ArgsSeconds).mssecond))
            {
                if (textLyric.Opacity == 1f)
                    return;

                scaleDownAnimation.Pause();
                scaleUpAnimation.Begin();

                // Highlight current line
            }
            else
            {
                if (textLyric.Opacity == 0.6f)
                    return;

                scaleUpAnimation.Pause();
                scaleDownAnimation.Begin();

                // Reset to default color when not active
                textLyric.Foreground = DefaultTextBrush;
            }
        }

      

        private void DefaultControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var data = this.DataContext;

            if (DataContext is string text)
            {
                textLyric.Text = text;
                textLyric.Foreground = DefaultTextBrush;
            }
            else if (DataContext is LyricsTimestamp lyricsTimestamp)
            {
                textLyric.Text = lyricsTimestamp.Line;

            }

            textLyric.Opacity = 0.6;
            BackgrD.Opacity = 0;
        }

        public bool IsLyricsTimestampAtPosition(int position)
        {
            if (DataContext is not LyricsTimestamp lyricsTimestamp)
                return false;

            if (lyricsTimestamp.Begin <= position && lyricsTimestamp.End >= position)
            {
                return true;
            }
            return false;
        }

        private void Grid_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (DataContext is not LyricsTimestamp lyricsTimestamp)
            {
                return;
            }

            AudioPlayer.mediaPlayer.Position = TimeSpan.FromMilliseconds(lyricsTimestamp.Begin);
        }

        private void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (DataContext is not LyricsTimestamp lyricsTimestamp)
                return;

            MoveBackDownStoryboard.Pause();
            MoveBackUpStoryboard.Begin();

            // Highlight on hover
            BackgrD.Opacity = 0.2;
        }

        private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            MoveBackUpStoryboard.Pause();
            MoveBackDownStoryboard.Begin();

            // Remove highlight
            BackgrD.Opacity = 0;
        }
    }
}