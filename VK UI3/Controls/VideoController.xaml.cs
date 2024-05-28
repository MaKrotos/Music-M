using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using MusicX.Core.Models;
using MusicX.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;
using Windows.Media.Playlists;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class VideoController : UserControl
    {
        Helpers.Animations.AnimationsChangeImage AnimationsChangeImage;
        public VideoController()
        {
            this.InitializeComponent();
            AnimationsChangeImage = new AnimationsChangeImage(imageVideo, this.DispatcherQueue);
          

            this.Unloaded += PlaylistControl_Unloaded;


            DataContextChanged += VideoController_DataContextChanged;


        }
        public bool setCOlorTheme { get; set; } = false;
        MusicX.Core.Models.Video video = null;
        private void VideoController_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            if (DataContext is MusicX.Core.Models.Video vid)
            {
                if (video == vid) return;
                video = vid;
                imageVideo.Source = null;
                AnimationsChangeImage.ChangeImageWithAnimation(video.Image.LastOrDefault().Url);



                MainText.Text = video.Title;
                ToolTip toolTip = new ToolTip();
                toolTip.Content = $"{video.Title}";
                if (video.Subtitle == null || video.Subtitle == "") {
                    SecondText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SecondText.Visibility = Visibility.Visible;
                    SecondText.Text = video.Subtitle;
                    toolTip.Content += $"\n{video.Subtitle}";
                }


      
                ToolTipService.SetToolTip(this, toolTip);
 
               
            }
        }

      

        private void PlaylistControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= PlaylistControl_Unloaded;
        }





        private async void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            HideAnimation.Pause();
            ShowAnimation.Begin();
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ShowAnimation.Pause();
            HideAnimation.Begin();
        }






    }
}
