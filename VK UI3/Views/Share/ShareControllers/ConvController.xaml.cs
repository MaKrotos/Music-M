using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using TagLib.Ape;
using VK_UI3.Resource;
using VK_UI3.VKs.IVK;
using VkNet.Model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Share.ShareControllers
{
    public sealed partial class ConvController : UserControl
    {
        public bool isDisabled { get; set; } = true;
        public ConvController()
        {
            this.InitializeComponent();
            animationsChangeImage = new Helpers.Animations.AnimationsChangeImage(userIcon, this.DispatcherQueue);
        }
        Helpers.Animations.AnimationsChangeImage animationsChangeImage;
        MessConv messConv;
        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            
            if (DataContext is not MessConv messConv)
            { return; }
            this.messConv = messConv;

            //ImagesGrid.Opacity = 0;


    


            switch (messConv.conversation.Peer.Type.ToString())
            {
                case "chat":
                    if (messConv.conversation.ChatSettings.Photo != null)
                    {
                        animationsChangeImage.ChangeImageWithAnimation(messConv.conversation.ChatSettings.Photo.JustGetPhoto.ToString());
                    }
                    else
                    {
                      
                    }
                    createAA(messConv.conversation.ChatSettings.Title);
                    nameTXT.Text = messConv.conversation.ChatSettings.Title;
                    break;
                case "user":
                    if (messConv.user.JustGetPhoto != null)
                        animationsChangeImage.ChangeImageWithAnimation(messConv.user.JustGetPhoto.ToString());
                    else { 
                      
                    }
                    createAA($"{messConv.user.FirstName} {messConv.user.LastName}");
                    nameTXT.Text = $"{messConv.user.FirstName} {messConv.user.LastName}";

                    if ((bool)messConv.user.Online)
                        onlineStatus.Visibility = Visibility.Visible;
                    else
                        onlineStatus.Visibility = Visibility.Collapsed;
                    break;
                case "group":
                    if (messConv.group.JustGetPhoto != null)
                    {
                        animationsChangeImage.ChangeImageWithAnimation(messConv.group.JustGetPhoto.ToString());
                    }
                    else
                    {
                        
                    }
                    createAA(messConv.group.Name);
                    nameTXT.Text = $"{messConv.group.Name}";
                    break;
                case "email":

                    break;
                default:
                    break;
            }

            ChatColors chatColors = new ChatColors();
            var color = chatColors.GetColorByNumber((int)messConv.conversation.Peer.LocalId % 6);
            gridBack.Background = new SolidColorBrush(color);



            if (isDisabled && !messConv.conversation.CanWrite.Allowed)
                this.IsEnabled = false;
            else
            {
                this.IsEnabled = true;
            }
    
        }

        private void createAA(string AAs)
        {
            var a = AAs.Split(" ");
            if (a.Count() == 0) return;
            if (a.Count() == 1 || a[1].Count() == 0)
            { 
                AAText.Text = a[0].Substring(0, 2);
                return;
            }
            AAText.Text = $"{a[0].Substring(0,1)}{a[1].Substring(0, 1)}";
        }

        private void StackPanel_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            MessagesAudio messagesAudio = new MessagesAudio(messConv: messConv, dispatcher: DispatcherQueue);
            MainView.OpenPlayList(messagesAudio);

        }

        private void StackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            HideAnimation.Pause();
            ShowAnimation.Begin();
        }

        private void StackPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            ShowAnimation.Pause();
            HideAnimation.Begin();
        }
    }
}
