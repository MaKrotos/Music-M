using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;
using VK_UI3.DB;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiShapesSetting : ComboBox
    {
        public ConfettiShapesSetting()
        {
            this.Header = "Формы частиц";
            this.Items.Add("square");
            this.Items.Add("circle");
            this.Items.Add("star");
            this.Items.Add("square,circle");
            this.Items.Add("square,circle,star");

            this.Loaded += ConfettiShapesSetting_Loaded;
            this.SelectionChanged += OnSelectionChanged;

            this.Style = Application.Current.Resources["DefaultComboBoxStyle"] as Style;

            AutomationProperties.SetName(this, "Формы частиц конфетти");
            AutomationProperties.SetHelpText(this, "Выберите форму частиц конфетти: квадраты, круги, звёзды или их комбинации");
        }

        private void ConfettiShapesSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var setting = SettingsTable.GetSetting("confettiShapes");
                if (setting != null)
                {
                    int index = this.Items.IndexOf(setting.settingValue);
                    if (index >= 0)
                    {
                        this.SelectedIndex = index;
                        return;
                    }
                }
                // По умолчанию square,circle
                this.SelectedIndex = 3;
            });
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedItem is string selectedShapes)
            {
                SettingsTable.SetSetting("confettiShapes", selectedShapes);
            }
        }
    }
}