using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;

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
        }

        private void DefaultControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.DataContextChanged -= DefaultControl_DataContextChanged;
            this.Unloaded -= DefaultControl_Unloaded;
        }

        private void DefaultControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var data = this.DataContext;

            if (DataContext is not string text)
                return;
            textLyric.Text = text;


        }
    }
}

