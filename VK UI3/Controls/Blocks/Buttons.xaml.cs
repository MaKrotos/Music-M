using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{

    public sealed partial class Buttons : UserControl
    {
        Block block { get { return DataContext as Block; } }
        public Buttons()
        {

            this.InitializeComponent();

            this.DataContextChanged += Buttons_DataContextChanged;
        }

        private void Buttons_DataContextChanged(Microsoft.UI.Xaml.FrameworkElement sender, Microsoft.UI.Xaml.DataContextChangedEventArgs args)
        {


        }
    }
}
