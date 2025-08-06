using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.VKs;
using VkNet;
using Windows.ApplicationModel;

namespace VK_UI3.Views.Settings
{
    class AppVersionText : UserControl
    {
        public AppVersionText()
        {
            this.Loaded += AppVersionText_Loaded;
            this.Unloaded += AppVersionText_Unloaded;
        }

        private void AppVersionText_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= AppVersionText_Loaded;
            this.Unloaded -= AppVersionText_Unloaded;
        }

        private void AppVersionText_Loaded(object sender, RoutedEventArgs e)
        {
            this.Content = GetTextBlock();
        }

        public TextBlock GetTextBlock()
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = GetAppVersion();
            return textBlock;
        }

        public string GetAppVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

    }

}
