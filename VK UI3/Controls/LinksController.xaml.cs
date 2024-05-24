using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MusicX.Core.Models;
using System;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class LinksController : UserControl
    {
        Helpers.Animations.AnimationsChangeImage AnimationsChangeImage;
        public LinksController()
        {
            this.InitializeComponent();
            AnimationsChangeImage = new AnimationsChangeImage(imageLink, this.DispatcherQueue);
          

            this.Unloaded += PlaylistControl_Unloaded;


            DataContextChanged += LinksController_DataContextChanged;


        }
        MusicX.Core.Models.Link link = null;
        private void LinksController_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            if (DataContext is MusicX.Core.Models.Link linked)
            {
                if (link == linked) return;
                link = linked;

                if (link.Image.Count != 0) 
                AnimationsChangeImage.ChangeImageWithAnimation(link.Image[0].Url);


                
            }
        }

      

        private void PlaylistControl_Unloaded(object sender, RoutedEventArgs e)
        {
     

            this.Unloaded -= PlaylistControl_Unloaded;
          
          

        }

       
       
    








    }
}
