using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Views.Settings
{
    class DeleteAllAudioProfile : Button
    {
        public DeleteAllAudioProfile()
        {
            this.CornerRadius = new CornerRadius(8);
            Click += DeleteAllAudioProfile_Click;
            Style style = Application.Current.Resources["DefaultButtonStyle"] as Style;
            this.Content = "Удалить все аудио из профиля";
        }
        Flyout myFlyout = new Flyout();
        private async void DeleteAllAudioProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (myFlyout.Content == null)
                {
                    TextBlock textBlock = new TextBlock();
                    StackPanel listView = new StackPanel();


                    textBlock.Text = "Вы уверены, что хотите удалить все аудио из своего профиля?";
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
            _= VKs.VK.deleteAllMusicFromProfile();
        }
    }
}
