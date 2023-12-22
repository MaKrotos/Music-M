using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using VK_UI3.Helpers;
using VK_UI3.Controllers;
using VK_UI3.Interfaces;

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
       private IVKGetAudio Tracks { 
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
