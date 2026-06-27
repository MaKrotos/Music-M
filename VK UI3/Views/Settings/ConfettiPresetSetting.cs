using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;
using VK_UI3.DB;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiPresetSetting : ComboBox
    {
        public ConfettiPresetSetting()
        {
            // Добавляем элементы выбора пресетов
            this.Items.Add("FireBasic");
            this.Items.Add("FireRandomDirection");
            this.Items.Add("FireRealistic");
            this.Items.Add("FireFireworks");
            this.Items.Add("FireStars");
            this.Items.Add("FireSnow");
            this.Items.Add("FireSchoolPride");
            this.Items.Add("Custom");

            this.Loaded += ConfettiPresetSetting_Loaded;
            this.SelectionChanged += OnSelectionChanged;

            this.Style = Application.Current.Resources["DefaultComboBoxStyle"] as Style;

            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Пресет конфетти");
            AutomationProperties.SetHelpText(this, "Выберите тип анимации конфетти: FireBasic, FireRandomDirection, FireRealistic, FireFireworks, FireStars, FireSnow, FireSchoolPride или Custom");
        }

        private void ConfettiPresetSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                // Загружаем сохранённое значение после того, как элемент добавлен в визуальное дерево
                var setting = SettingsTable.GetSetting("confettiPreset");
                if (setting != null)
                {
                    // Ищем индекс выбранного элемента
                    int index = this.Items.IndexOf(setting.settingValue);
                    if (index >= 0)
                    {
                        this.SelectedIndex = index;
                    }
                    else
                    {
                        this.SelectedIndex = 0; // По умолчанию FireBasic
                    }
                }
                else
                {
                    // Значение по умолчанию
                    SettingsTable.SetSetting("confettiPreset", "FireBasic");
                    this.SelectedIndex = 0;
                }
            });
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedItem is string selectedPreset)
            {
                SettingsTable.SetSetting("confettiPreset", selectedPreset);
            }
        }
    }
}