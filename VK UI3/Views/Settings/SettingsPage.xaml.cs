using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
            
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _ = UpdateAudioCacheSizeAsync();
        }

        private async Task UpdateAudioCacheSizeAsync()
        {
            AudioCacheCalcProgress.IsActive = true;
            AudioCacheCalcProgress.Visibility = Visibility.Visible;
            AudioCacheSizeText.Text = "Вычисление...";
            
            double sizeMb = await VK_UI3.Services.AudioCacheService.GetCacheSizeInMbAsync();
            AudioCacheSizeText.Text = $"{sizeMb} МБ";
            
            AudioCacheCalcProgress.IsActive = false;
            AudioCacheCalcProgress.Visibility = Visibility.Collapsed;
        }

        private async void ClearAudioCacheButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                // Блокируем кнопку от двойного нажатия и меняем текст
                btn.IsEnabled = false;
                btn.Content = "Очистка...";

                await VK_UI3.Services.AudioCacheService.ClearCacheAsync();

                // Обновляем текст с размером сразу после очистки
                await UpdateAudioCacheSizeAsync();

                // Показываем сообщение об успехе на 2 секунды
                btn.Content = "Кэш очищен";
                await Task.Delay(2000);

                // Возвращаем кнопку в исходное состояние
                btn.Content = "Очистить кэш аудио";
                btn.IsEnabled = true;
            }
        }
    }
}
