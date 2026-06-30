using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Media;

namespace VK_UI3.Views.Settings
{
    class UserHashText : UserControl
    {
        public UserHashText()
        {
            this.Loaded += UserHashText_Loaded;
            this.Unloaded += UserHashText_Unloaded;
        }

        private void UserHashText_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserHashText_Loaded;
            this.Unloaded -= UserHashText_Unloaded;
        }

        private void UserHashText_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Content = GetControl();
            }
            catch
            {
            }
        }

        public StackPanel GetControl()
        {
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8
            };

            TextBlock textBlock = new TextBlock();
            textBlock.Text = GetUserHash();
            AutomationProperties.SetName(textBlock, "Хэш пользователя");
            AutomationProperties.SetHelpText(textBlock, "Отображает хэш текущего пользователя");
            
            Button copyButton = new Button
            {
                Content = "Копировать",
                FontSize = 12,
                Padding = new Thickness(8, 4, 8, 4)
            };
            
            copyButton.Click += (s, e) =>
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(textBlock.Text);
                Clipboard.SetContent(dataPackage);
            };

            panel.Children.Add(textBlock);
            panel.Children.Add(copyButton);

            return panel;
        }

        public string GetUserHash()
        {
            try
            {
                var activeAccount = AccountsDB.activeAccount;
                if (activeAccount == null) return "Аккаунт не выбран";
                
                return activeAccount.GetHash();
            }
            catch
            {
                return "Ошибка";
            }
        }
    }
}
