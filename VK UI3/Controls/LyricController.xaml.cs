using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using MusicX.Core.Models;
using ProtoBuf.WellKnownTypes;
using VK_UI3.Views.ModalsPages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class LyricController : UserControl
    {
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
            }
            else
            {
                if (textLyric.Opacity == 0.8f)
                    return;
                scaleUpAnimation.Pause();
                scaleDownAnimation.Begin();
            }
        }

        private void DefaultControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var data = this.DataContext;

            if (DataContext is string text)
            {
                textLyric.Text = text;

            }
            if (DataContext is LyricsTimestamp lyricsTimestamp)
            {
                textLyric.Text = lyricsTimestamp.Line;
            }
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

    }
}

