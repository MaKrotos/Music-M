using MusicX.Core.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controllers
{
    public class NavSettings
    {
        public string MyMusicItem { get; set; }
        public string Icon { get; set; }

        public Section section { get; set; }

    }

    public partial class NavMenuController
    {



        public NavSettings navSettings
        {
            get; set;
        }



        public NavMenuController()
        {
        }
    }
}
