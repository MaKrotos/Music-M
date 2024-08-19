using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.VKs;
using VkNet;

namespace VK_UI3.Views.Settings
{
    class ArchivateTracks : Button
    {
        public ArchivateTracks()
        {
            this.CornerRadius = new CornerRadius(8);
            Click += ArchivateTracks_Click;
            Style style = Application.Current.Resources["DefaultButtonStyle"] as Style;
            this.Content = "Заархивировать треки профиля";
        }
        Flyout myFlyout = new Flyout();
        private async void ArchivateTracks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (myFlyout.Content == null)
                {
                    TextBlock textBlock = new TextBlock();
                    StackPanel listView = new StackPanel();


                    textBlock.Text = "Вы уверены, что хотите заархивировать все аудио в своём профиле? Все треки из Вашего профиля перенесутся в автоматически созданные плейлисты.";
                    textBlock.MaxWidth = 300;
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    listView.Children.Add(textBlock);

                    Button button = new Button();
                    button.Margin = new Thickness(0, 15, 0 ,0);
                    button.Content = "Да, уверен!";
                    listView.Children.Add(button);
                    myFlyout.Content = listView;
                  
                    button.Click += Button_Click;
                }
                myFlyout.ShowAt(this);
            }
            catch (Exception ex)
            {
              
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _= new KrotosVK().archiveTracksFromProfileAsync(VK.api);
            myFlyout.Hide();
        }
    }
}
