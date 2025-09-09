using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using MusicX.Core.Services;
using System.Diagnostics;
using System.Drawing;
using VK_UI3.Services;
using VK_UI3.VKs;
using VK_UI3.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class PlaceholderBlockControl : UserControl
    {
        public PlaceholderBlockControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += OwnerCell_DataContextChanged;
            animationsChangeImage = new Helpers.Animations.AnimationsChangeImage(icon, this.DispatcherQueue);
        }
        Helpers.Animations.AnimationsChangeImage animationsChangeImage; 
        Block block = null;
        private void OwnerCell_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            
            block = (Block)DataContext;


            if (block.Placeholders == null || block.Placeholders.Count == 0)
            {

                this.icon.Visibility = Visibility.Collapsed;
                this.Title.Visibility = Visibility.Collapsed;
                ActionButton.Visibility = Visibility.Collapsed;
                return;
            }
            var placeholder = block.Placeholders[0];



            if (placeholder.Icons.Count > 0)
            {
                var image = placeholder.Icons.MaxBy(i => i.Width);
                this.icon.Visibility = Visibility.Visible;
                animationsChangeImage.ChangeImageWithAnimation(image.Url);
            
            }
            else
            {
                this.icon.Visibility = Visibility.Collapsed;
            }

            this.Title.Text = placeholder.Title;
            if (placeholder.Title == String.Empty) this.Title.Visibility = Visibility.Collapsed; else Title.Visibility = Visibility.Visible;
            this.Text.Text = placeholder.Text;

            if (placeholder.Buttons.Count > 0)
            {
                var button = placeholder.Buttons.FirstOrDefault();

                this.ActionButton.Content = button.Title;
                this.buttonAction = button;
                ActionButton.Content = button.Title;
            }
            else
            {
                ActionButton.Visibility = Visibility.Collapsed;
            }

        }

      
        private MusicX.Core.Models.Button buttonAction;
        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (buttonAction.Action.Type == "custom_open_browser")
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = buttonAction.Action.Url
                    });

                    return;
                }
                var music = await VK.vkService.GetAudioCatalogAsync(buttonAction.Action.Url);
                MainView.OpenSection(music.Catalog.DefaultSection);
            }
            catch (Exception ex)
            {

            
            }

        }
    }
}
