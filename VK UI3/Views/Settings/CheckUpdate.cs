using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    class CheckUpdate : Button
    {
        public CheckUpdate()
        {
            this.CornerRadius = new CornerRadius(8);
            Click += CheckUpdate_Click;
            Style style = Application.Current.Resources["DefaultButtonStyle"] as Style;
            this.Content = "Проверить обновления";
            
            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Проверить обновления");
            AutomationProperties.SetHelpText(this, "Проверяет наличие новых версий приложения");
        }

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsEnabled = false;
                var a = await MainWindow.mainWindow.checkUpdate();
                if (!a)
                {

                    Flyout myFlyout = new Flyout();

                    // Добавьте элементы в меню
                    TextBlock firstItem = new TextBlock { Text = "Обновлений не найдено" };
                    
                    // Добавляем свойства доступности для текстового блока
                    AutomationProperties.SetName(firstItem, "Результат проверки обновлений");
                    AutomationProperties.SetHelpText(firstItem, "Информирует о том, что обновления не найдены");

                    myFlyout.Content = firstItem;


                    // Покажите всплывающее меню
                    myFlyout.ShowAt(this);

                    // Создайте таймер, который закроет меню через 5 секунд
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(10);
                    timer.Tick += (s, e) =>
                    {
                        myFlyout.Hide();
                        timer.Stop();
                        this.IsEnabled = true;
                    };
                    timer.Start();
                }
                else
                {
                    this.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MenuFlyout myFlyout = new MenuFlyout();

                // Добавьте элементы в меню
                MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = $"Произошла ошибка. Проверьте настройки сети.\n{ex.Message}" };
                
                // Добавляем свойства доступности для элемента меню
                AutomationProperties.SetName(firstItem, "Ошибка проверки обновлений");
                AutomationProperties.SetHelpText(firstItem, $"Произошла ошибка при проверке обновлений: {ex.Message}");

                myFlyout.Items.Add(firstItem);


                // Покажите всплывающее меню
                myFlyout.ShowAt(this);

                // Создайте таймер, который закроет меню через 5 секунд
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(10);
                timer.Tick += (s, e) =>
                {
                    myFlyout.Hide();
                    timer.Stop();
                  
                };
                timer.Start();
                this.IsEnabled = true;
            }
        }
    }
}
