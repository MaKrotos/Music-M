using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VK_UI3.DB;
using VK_UI3.DownloadTrack;
using Microsoft.UI.Xaml.Automation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Settings
{
    public sealed class StartAllTaskSetting : CheckBox
    {
        public StartAllTaskSetting()
        {


            this.Content = "Запускать все таски автоматически";

            this.Checked += StartUpSetting_Checked;
            this.Unchecked += StartUpSetting_Unchecked;
            this.Loaded += StartUpSetting_Loaded;

            // Получение стиля из ресурсов
            Style style = Application.Current.Resources["DefaultCheckBoxStyle"] as Style;

            // Установка стиля
            this.Style = style;
            
            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Запускать все задачи автоматически");
            AutomationProperties.SetHelpText(this, "Автоматически запускает все задачи скачивания при запуске приложения");
        }

        private void StartUpSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var set = SettingsTable.GetSetting("TaskALL");
                this.IsChecked = set != null;
            });
        }

        private void StartUpSetting_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsTable.RemoveSetting("TaskALL");
            PlayListDownload.ResumeOnlyFirst();

        }

        private void StartUpSetting_Checked(object sender, RoutedEventArgs e)
        {
            SettingsTable.SetSetting("TaskALL", "1");
            PlayListDownload.ResumeAll();
        }
    }
}
