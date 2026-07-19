using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;

namespace VK_UI3.Views.Controls
{
    public sealed partial class SocialLinkButtonControl : UserControl
    {
        public SocialLinkButtonControl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty SocialLinkProperty = DependencyProperty.Register(
            nameof(SocialLink), typeof(SocialLinkItem), typeof(SocialLinkButtonControl), new PropertyMetadata(null));

        public SocialLinkItem SocialLink
        {
            get => (SocialLinkItem)GetValue(SocialLinkProperty);
            set => SetValue(SocialLinkProperty, value);
        }

        private async void SocialButton_Click(object sender, RoutedEventArgs e)
        {
            if (SocialLink != null && !string.IsNullOrWhiteSpace(SocialLink.Url))
            {
                try
                {
                    var uri = new Uri(SocialLink.Url);
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error opening social link: {ex.Message}");
                }
            }
        }
    }
}