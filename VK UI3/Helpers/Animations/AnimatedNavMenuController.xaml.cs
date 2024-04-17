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
using VK_UI3.Controllers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Helpers.Animations
{
    public sealed partial class AnimatedNavMenuController : NavMenuController
    {
        public AnimatedNavMenuController()
        {
            this.InitializeComponent();

            this.Loaded += AnimatedNavMenuController_Loaded;
        }

        private void AnimatedNavMenuController_Loaded(object sender, RoutedEventArgs e)
        {
            HideAnimation.Pause();
            ShowAnimation.Begin();
        }

        public void delete()
        {
            ShowAnimation.Begin();
            HideAnimation.Completed += HideAnimation_Completed;
            HideAnimation.Begin();
        }
        public EventHandler deleted;

        private void HideAnimation_Completed(object sender, object e)
        {
           
            deleted.Invoke(this, EventArgs.Empty);

      
            HideAnimation.Completed -= HideAnimation_Completed;
        }

     
    }
}
