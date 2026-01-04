using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Timers;
using VK_UI3.Services;
using VK_UI3.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public class ExpBanner
    {
        public CatalogBanner catalogBanner;


        public bool selected = false;

        public ExpBanner(CatalogBanner catalogBanner)
        {
            this.catalogBanner = catalogBanner;
        }

        public event EventHandler showedClick;


        public void invokeShowedClick(ExpBanner expBanner) 
        {
            if (expBanner == this)
            {
                selected = true;
            }
            else selected = false;

            showedClick.Invoke(expBanner, EventArgs.Empty);
        }
    }



    public sealed partial class BigBannerControl : UserControl
    {


        System.Timers.Timer timer = new System.Timers.Timer(7000);
        public BigBannerControl()
        {
            this.InitializeComponent();
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            this.DataContextChanged += BigBannerControl_DataContextChanged;
            this.changeImage = new Helpers.Animations.AnimationsChangeImage(BannerImage, DispatcherQueue);
            titleText = new Helpers.Animations.AnimationsChangeText(Title, DispatcherQueue);
            SubTitleText = new Helpers.Animations.AnimationsChangeText(SubTitle, DispatcherQueue);
            DescriptionText = new Helpers.Animations.AnimationsChangeText(Description, DispatcherQueue);
            this.Loaded += BigBannerControl_Loaded;
            this.Unloaded += BigBannerControl_Unloaded;

            
        }

        private void BigBannerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in banners)
            {
                item.showedClick -= Banner_showedClick;
            }
            this.Loaded -= BigBannerControl_Loaded;
            this.Unloaded -= BigBannerControl_Unloaded;
            this.DataContextChanged -= BigBannerControl_DataContextChanged;
            timer.Elapsed -= OnTimedEvent;
        }

        private void BigBannerControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (banners.Count == 0) return;
            int l_last_index = lastIndex + 1;
            if (l_last_index > banners.Count - 1)
                l_last_index = 0;
            banners[l_last_index].invokeShowedClick(banners[l_last_index]);
        }

        public List<ExpBanner> banners = new List<ExpBanner>();
        Helpers.Animations.AnimationsChangeImage changeImage;
        private void BigBannerControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext is Block block)
            {
                if (block.Banners.Count() < 2)
                {

                    timer.Stop();
                    myControl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    foreach (var item in block.Banners)
                    {
                        ExpBanner banner = new ExpBanner(item);
                        if (banners.Count == 0)
                        {
                            banner.selected = true;
                        }
                        banner.showedClick += Banner_showedClick;
                        banners.Add(banner);
                    }
                }
            }
            else return;

            this.Title.Text = block.Banners[0].Title;
            this.SubTitle.Text = block.Banners[0].Text;

            this.Description.Text = block.Banners[0].SubText;
            changeImage.ChangeImageWithAnimation(block.Banners[0].Images.Last().Url);

            if (banners.Count == 0 || (banners[lastIndex].catalogBanner.ClickAction?.Action ?? banners[lastIndex].catalogBanner.Buttons?.FirstOrDefault()?.Action) is not { } action)
            {
                ActionCuratorButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ActionCuratorButton.Visibility = Visibility.Visible;
            }

        }
        Helpers.Animations.AnimationsChangeText titleText;
        Helpers.Animations.AnimationsChangeText SubTitleText;
        Helpers.Animations.AnimationsChangeText DescriptionText;

        int lastIndex = 0;
        private void Banner_showedClick(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Start();
            if (!(sender is ExpBanner expBanner))
                return;
       
            var index = banners.IndexOf(expBanner);
            if (lastIndex == index) return;
            lastIndex = index;

            titleText.ChangeTextWithAnimation(banners[index].catalogBanner.Title);
            SubTitleText.ChangeTextWithAnimation(banners[index].catalogBanner.SubText);
            DescriptionText.ChangeTextWithAnimation(banners[index].catalogBanner.Text);
       
            changeImage.ChangeImageWithAnimation(banners[index].catalogBanner.Images.Last().Url);

            foreach (var item in banners)
            {
                item.invokeShowedClick(expBanner);
            }
            myControl.scrollToIndex(lastIndex);
        }

       
        Helpers.Animations.AnimationsChangeFontIcon animationsChangeFont;


        private async void ActionCuratorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((banners[lastIndex].catalogBanner.ClickAction?.Action ?? banners[lastIndex].catalogBanner.Buttons?.FirstOrDefault()?.Action) is not { } action)
                    return;

                var url = new Uri(action.Url);

                if (url.Segments.LastOrDefault() is not { } lastSegment || !lastSegment.Contains('_'))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = action.Url
                    });
                    return;
                }

                var data = lastSegment.Split("_");

                var ownerId = long.Parse(data[0]);
                var playlistId = long.Parse(data[1]);
                var accessKey = data.Length == 2 ? string.Empty : data[2];


                MainView.OpenPlayList(playlistId, ownerId, accessKey);
            }
            catch (Exception ex)
            {

                Process.Start(new ProcessStartInfo
                {
                    FileName = banners[lastIndex].catalogBanner.ClickAction?.Action.Url,
                    UseShellExecute = true
                });
            }

        }
    }
}
