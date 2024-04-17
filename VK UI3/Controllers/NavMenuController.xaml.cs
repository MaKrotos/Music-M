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
using Windows.Foundation;
using Windows.Foundation.Collections;
using static VK_UI3.DB.AccountsDB;

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
