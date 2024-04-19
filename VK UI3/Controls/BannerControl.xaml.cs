using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Linq;

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

        }

        private void BannerControl_Unloaded(object sender, RoutedEventArgs e)
        {

            this.BannerCover = null;

            this.BannerTitle = null;
            this.BannerText = null;
            this.Loaded += UserControl_Loaded;
        }

        public static readonly DependencyProperty BannerProperty =
          DependencyProperty.Register("Banner", typeof(CatalogBanner), typeof(BannerControl), new PropertyMetadata(new CatalogBanner()));

        public CatalogBanner Banner
        {
            get { return (CatalogBanner)GetValue(BannerProperty); }
            set
            {
                SetValue(BannerProperty, value);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                BannerTitle.Text = Banner.Title;
                BannerText.Text = Banner.Text;
                //   BannerCover.Source = new BitmapImage(new Uri(Banner.Images.Last().Url));
            }
            catch (Exception ex)
            {

            }
        }

        private void Card_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                var url = new Uri(Banner.ClickAction.Action.Url);

                var data = url.Segments.LastOrDefault().Split("_");

                var ownerId = long.Parse(data[0]);
                var playlistId = long.Parse(data[1]);
                var accessKey = data[2];

                //    var notificationService = StaticService.Container.GetRequiredService<Services.NavigationService>();

                //    notificationService.OpenExternalPage(new PlaylistView(playlistId, ownerId, accessKey));

            }
            catch (Exception ex)
            {


            }

        }
    }
}
