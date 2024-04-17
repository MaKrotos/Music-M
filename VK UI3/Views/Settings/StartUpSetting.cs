using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Settings
{
    public sealed class StartUpSetting : CheckBox
    {
        public StartUpSetting()
        {


            this.Content = "Добавить приложение в автозагрузку";

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
                this.IsChecked = await Helpers.StartupManager.IsAppInStartupAsync();
            });
        }

        private void StartUpSetting_Unchecked(object sender, RoutedEventArgs e)
        {
            Helpers.StartupManager.DisableStartupAsync();
        }

        private void StartUpSetting_Checked(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {

                Helpers.StartupManager.EnableStartupAsync();

            });
        }
    }
}
