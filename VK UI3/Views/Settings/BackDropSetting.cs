using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using VK_UI3.DB;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Settings
{
    public sealed class BackDropSetting : CheckBox
    {
        public BackDropSetting()
        {


            this.Content = "Выключить прозрачность";

            this.Checked += StartUpSetting_Checked;
            this.Unchecked += StartUpSetting_Unchecked;
            this.Loaded += StartUpSetting_Loaded;

            // Получение стиля из ресурсов
            Style style = Application.Current.Resources["DefaultCheckBoxStyle"] as Style;

            // Установка стиля
            this.Style = style;
        }

        private void StartUpSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var set = SettingsTable.GetSetting("backDrop");
                this.IsChecked = set != null;
            });
        }

        private void StartUpSetting_Unchecked(object sender, RoutedEventArgs e)
        {

            MainWindow.dispatcherQueue.TryEnqueue(() =>
            {
                SettingsTable.RemoveSetting("backDrop");
                MainWindow.updateMica?.Invoke(this, EventArgs.Empty);

            });
        }

        private void StartUpSetting_Checked(object sender, RoutedEventArgs e)
        {

            MainWindow.dispatcherQueue.TryEnqueue(() =>
            {
                SettingsTable.SetSetting("backDrop", "1");
                MainWindow.updateMica?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
