using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using VK_UI3.Helpers;

namespace VK_UI3.Views.Settings
{
    public sealed class CreateDesktopShortcut : Button
    {
        public CreateDesktopShortcut()
        {
            this.CornerRadius = new CornerRadius(8);
            Click += CreateDesktopShortcut_Click;
            Style style = Application.Current.Resources["DefaultButtonStyle"] as Style;
            this.Style = style;
            this.Content = "Создать ярлык на рабочем столе";
            
            LoadShortcutState();
        }

        private async void LoadShortcutState()
        {
            var exists = await ShortcutManager.IsDesktopShortcutExistsAsync();
            UpdateButtonState(exists);
        }

        private void UpdateButtonState(bool shortcutExists)
        {
            if (shortcutExists)
            {
                this.Content = "Ярлык уже создан";
                this.IsEnabled = false;
            }
            else
            {
                this.Content = "Создать ярлык на рабочем столе";
                this.IsEnabled = true;
            }
        }

        private async void CreateDesktopShortcut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsEnabled = false;
                this.Content = "Создание ярлыка...";

                var success = await ShortcutManager.CreateDesktopShortcutAsync();

                if (success)
                {
                    ShowNotification("Ярлык успешно создан на рабочем столе!", true);
                    UpdateButtonState(true);
                }
                else
                {
                    ShowNotification("Не удалось создать ярлык. Попробуйте еще раз.", false);
                    this.IsEnabled = true;
                    this.Content = "Создать ярлык на рабочем столе";
                }
            }
            catch (Exception ex)
            {
                ShowNotification($"Произошла ошибка: {ex.Message}", false);
                this.IsEnabled = true;
                this.Content = "Создать ярлык на рабочем столе";
            }
        }

        private void ShowNotification(string message, bool isSuccess)
        {
            Flyout myFlyout = new Flyout();

            var container = new StackPanel();
            
            var textBlock = new TextBlock 
            { 
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 300
            };
            
            container.Children.Add(textBlock);
            myFlyout.Content = container;

            myFlyout.ShowAt(this);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (s, e) =>
            {
                myFlyout.Hide();
                timer.Stop();
            };
            timer.Start();
        }
    }
} 