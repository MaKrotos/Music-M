using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsEnabled = false;
                var a = await MainWindow.mainWindow.checkUpdate();
                if (!a)
                {

                    MenuFlyout myFlyout = new MenuFlyout();

                    // Добавьте элементы в меню
                    MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = "Обновлений не найдено" };

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
