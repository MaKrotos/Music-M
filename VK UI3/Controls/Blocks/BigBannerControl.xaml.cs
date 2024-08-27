using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MusicX.Core.Models;
using System.Collections.Generic;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class BigBannerControl : UserControl
    {
        public BigBannerControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += BigBannerControl_DataContextChanged;
            this.changeImage = new Helpers.Animations.AnimationsChangeImage(BannerImage, DispatcherQueue);
        }
        public List<CatalogBanner> banners;
        Helpers.Animations.AnimationsChangeImage changeImage;
        private void BigBannerControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext is Block block)
            {
                banners = block.Banners;
            }
            else return;

            this.Title.Text = Banners[0].Title;
            this.SubTitle.Text = Banners[0].Text;

            this.Description.Text = Banners[0].SubText;
            changeImage.ChangeImageWithAnimation(Banners[0].Images.Last().Url);

        }
        Helpers.Animations.AnimationsChangeText animationsChangeText;
        Helpers.Animations.AnimationsChangeFontIcon animationsChangeFont;

        public List<CatalogBanner> Banners => ((Block)DataContext).Banners;

        private async void ActionCuratorButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
