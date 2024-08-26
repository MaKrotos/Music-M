using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using MusicX.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VK_UI3.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class CuratorBannerBlockControl : UserControl
    {
        public CuratorBannerBlockControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += CuratorBannerBlockControl_DataContextChanged;
            animationsChangeText = new Helpers.Animations.AnimationsChangeText(this.BTNText, this.DispatcherQueue);
            animationsChangeFont = new Helpers.Animations.AnimationsChangeFontIcon(this.BTNIcon, this.DispatcherQueue);            
        }
        public Block Block => (Block)DataContext;
        private void CuratorBannerBlockControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext is not MusicX.Core.Models.Block)
                return;


            new Helpers.Animations.AnimationsChangeImage(CuratorBannerImage, DispatcherQueue).ChangeImageWithAnimation(Block.Curators[0].Photo[2].Url);


            CuratorText.Text = Block.Curators[0].Name;
        

            if (Block.Curators[0].IsFollowed)
            {
                BTNText.Text = "Отписаться";
                BTNIcon.Glyph = "\uE74D";
            }
            else
            {
                BTNText.Text = "Подписаться";
                BTNIcon.Glyph = "\uF8AA";
            }
        }
        Helpers.Animations.AnimationsChangeText animationsChangeText;
        Helpers.Animations.AnimationsChangeFontIcon animationsChangeFont;
     

        private async void ActionCuratorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vkService = VKs.VK.vkService;
                if (Block.Curators[0].IsFollowed)
                {
                    ActionCuratorButton.IsEnabled = false;

                    animationsChangeText.ChangeTextWithAnimation("Секунду...");
                    animationsChangeFont.ChangeFontIconWithAnimation("\uE823");


                    await vkService.UnfollowCurator(Block.Curators[0].Id);

                    ActionCuratorButton.IsEnabled = true;

                    Block.Curators[0].IsFollowed = false;

                    animationsChangeText.ChangeTextWithAnimation("Подписаться");
                    animationsChangeFont.ChangeFontIconWithAnimation("\uF8AA");



                }
                else
                {
                    ActionCuratorButton.IsEnabled = false;
                    animationsChangeText.ChangeTextWithAnimation("Секунду...");
                    animationsChangeFont.ChangeFontIconWithAnimation("\uE823");


                    await vkService.FollowCurator(Block.Curators[0].Id);

                    ActionCuratorButton.IsEnabled = true;
                    Block.Curators[0].IsFollowed = true;
                    BTNText.Text = "Отписаться";
                    BTNIcon.Glyph = "\uE74D";
                }
            }
            catch (Exception ex)
            {


            }
        }
    }
}
