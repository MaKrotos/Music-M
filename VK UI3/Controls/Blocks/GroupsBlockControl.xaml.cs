using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using MusicX.Core.Models;
using VK_UI3.Helpers.Animations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using VK_UI3.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class GroupsBlockControl : UserControl
    {
        public GroupsBlockControl()
        {
            this.InitializeComponent();
            animationsChangeImage = new AnimationsChangeImage(GroupImage, this.DispatcherQueue);
            this.Loaded += GroupsBlockControl_Loaded;
        }

        AnimationsChangeImage animationsChangeImage;
        private void GroupsBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Block block)
                return;
            animationsChangeImage.ChangeImageWithAnimation(block.Groups[0].JustGetPhoto);
            //if (block.Groups[0].Photo100 != null) GroupImage.ImageSource = new BitmapImage(new Uri(block.Groups[0].Photo100));

            GroupName.Text = block.Groups[0].Name;
            GroupSub.Text = block.Groups[0].MembersCount.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Block block)
                return;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://vk.ru/" + block.Groups[0].ScreenName,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {


            }
        }
    }
}
