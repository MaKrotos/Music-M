using Microsoft.UI.Xaml.Controls;
using VK_UI3.Controllers;
using VK_UI3.VKs.IVK;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayList : Page
    {
        //  public static List<ExtendedAudio> tracklist = new List<ExtendedAudio>();
        private IVKGetAudio Tracks
        {
            get
            {
                return AudioPlayer.iVKGetAudio;
            }
            set { AudioPlayer.iVKGetAudio = value; }

        }
        public PlayList()
        {
            this.InitializeComponent();
        }
    }
}
