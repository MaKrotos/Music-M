using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Settings
{
    public sealed class HighlightPlayedTracks : CheckBox
    {
        public HighlightPlayedTracks()
        {
            this.Content = "Выделять полностью прослушанные треки";

            this.Checked += HighlightPlayedTracks_Checked;
            this.Unchecked += HighlightPlayedTracks_Unchecked;
            this.Loaded += HighlightPlayedTracks_Loaded;

            // Получение стиля из ресурсов
            Style style = Application.Current.Resources["DefaultCheckBoxStyle"] as Style;
            
            // Установка стиля
            this.Style = style;
            
            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Выделять полностью прослушанные треки");
            AutomationProperties.SetHelpText(this, "Выделяет цветом треки, которые были полностью прослушаны");
        }

        private void HighlightPlayedTracks_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
               var setting = DB.SettingsTable.GetSetting("HighlightPlayedTracks");

                if (setting == null)
                    return;
                this.IsChecked = setting.settingValue.Equals("1") ? true : false;
            });
        }

        private void HighlightPlayedTracks_Unchecked(object sender, RoutedEventArgs e)
        {
            DB.SettingsTable.SetSetting("HighlightPlayedTracks", "0");
        }

        private void HighlightPlayedTracks_Checked(object sender, RoutedEventArgs e)
        {
            DB.SettingsTable.SetSetting("HighlightPlayedTracks", "1");
        }
    }
}