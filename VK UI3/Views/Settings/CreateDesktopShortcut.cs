using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using Microsoft.UI.Xaml.Automation;

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
            
            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Создать ярлык на рабочем столе");
            AutomationProperties.SetHelpText(this, "Создает ярлык приложения на рабочем столе");

            LoadShortcutState();
        }

        private async void LoadShortcutState()
        {
            try
            {
                var exists = await ShortcutManager.IsDesktopShortcutExistsAsync();
                UpdateButtonState(exists);
            }
            catch
            {
               
            }
        }

        private void UpdateButtonState(bool shortcutExists)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                if (shortcutExists)
                {
                    this.Content = "Ярлык уже создан";
                    this.IsEnabled = false;
                    AutomationProperties.SetName(this, "Ярлык уже создан");
                    AutomationProperties.SetHelpText(this, "Ярлык приложения уже существует на рабочем столе");
                }
                else
                {
                    this.Content = "Создать ярлык на рабочем столе";
                    this.IsEnabled = true;
                    AutomationProperties.SetName(this, "Создать ярлык на рабочем столе");
                    AutomationProperties.SetHelpText(this, "Создает ярлык приложения на рабочем столе");
                }
            });
        }

        private async void CreateDesktopShortcut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsEnabled = false;
                this.Content = "Создание ярлыка...";
                AutomationProperties.SetName(this, "Создание ярлыка");
                AutomationProperties.SetHelpText(this, "Выполняется создание ярлыка приложения на рабочем столе");

                var success = await ShortcutManager.CreateDesktopShortcutAsync();

                if (!Packaged.IsPackaged())
                {
                   _= ShortcutManager.CreateStartMenuShortcutAsync();
                }
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
                    AutomationProperties.SetName(this, "Создать ярлык на рабочем столе");
                    AutomationProperties.SetHelpText(this, "Создает ярлык приложения на рабочем столе");
                }
            }
            catch (Exception ex)
            {
                ShowNotification($"Произошла ошибка: {ex.Message}", false);
                this.IsEnabled = true;
                this.Content = "Создать ярлык на рабочем столе";
                AutomationProperties.SetName(this, "Создать ярлык на рабочем столе");
                AutomationProperties.SetHelpText(this, "Создает ярлык приложения на рабочем столе");
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
            
            // Добавляем свойства доступности для текстового блока уведомления
            AutomationProperties.SetName(textBlock, "Уведомление о создании ярлыка");
            AutomationProperties.SetHelpText(textBlock, message);
            
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