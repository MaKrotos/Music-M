using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

using VK_UI3.Controls.Blocks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class BannerControl : UserControl
    {
       
        public BannerControl()
        {
            this.InitializeComponent();
            this.Unloaded += BannerControl_Unloaded;
            this.changeImage = new Helpers.Animations.AnimationsChangeImage(BannerCover, DispatcherQueue);
            this.DataContextChanged += BannerControl_DataContextChanged;

          
        }

       

        bool connected = false;

        private void BannerControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {

            if (!(DataContext is ExpBanner banner)) return;

            if (Banner != banner)
            {
                BannerCover.Source = null;
                if (connected)
                {
                    Banner.showedClick -= Banner_showedClick;
                }
            }

            this.Banner = banner;
            Banner.showedClick += Banner_showedClick;
            connected = true;

            BannerTitle.Text = banner.catalogBanner.Title;
            BannerText.Text = banner.catalogBanner.Text;
            changeImage.ChangeImageWithAnimation(banner.catalogBanner.Images.Last().Url);
            if (banner.selected)
            {
                BorderGrid.Opacity = 1;
            }
            else
            {
                BorderGrid.Opacity = 0;
            }
        }

        private void Banner_showedClick(object sender, EventArgs e)
        {
            if (!(sender is ExpBanner expBanner)) return;
            if (expBanner == Banner)
            {
                this.DispatcherQueue.TryEnqueue(() => {
                    FadeOutStoryboard.Pause();
                    FadeInStoryboard.Begin();
                });
              
            }
            else
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    FadeInStoryboard.Pause();
                    FadeOutStoryboard.Begin();
                });
            }
        }

        ExpBanner Banner;

        private void BannerControl_Unloaded(object sender, RoutedEventArgs e)
        {

            this.Unloaded -= BannerControl_Unloaded;
            this.DataContextChanged -= BannerControl_DataContextChanged;
            Banner.showedClick -= Banner_showedClick;
   

        }

        Helpers.Animations.AnimationsChangeImage changeImage;
      

   

        private void Card_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {

                Banner.invokeShowedClick(Banner);

            

            }
            catch (Exception ex)
            {

            }

        }

       

        private void Card_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            FadeOutStoryboard.Pause();
            FadeInStoryboard.Begin();
        }

        private void Card_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

            if (Banner.selected) return;
            FadeInStoryboard.Pause();
            FadeOutStoryboard.Begin();
        }
       
  
    }
}
