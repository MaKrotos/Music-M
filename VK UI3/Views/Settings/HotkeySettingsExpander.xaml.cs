using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;
using System;
using System.Diagnostics;
using VK_UI3.Services;
using VK_UI3.Services.Player;

namespace VK_UI3.Views.Settings
{
    /// <summary>
    /// Панель с настройками всех горячих клавиш для управления воспроизведением.
    /// </summary>
    public sealed partial class HotkeySettingsExpander : StackPanel
    {
        public HotkeySettingsExpander()
        {
            this.InitializeComponent();
            this.Loaded += HotkeySettingsExpander_Loaded;

            AutomationProperties.SetName(this, "Горячие клавиши");
            AutomationProperties.SetHelpText(this, "Настройки горячих клавиш для управления воспроизведением музыки");
        }

        private void HotkeySettingsExpander_Loaded(object sender, RoutedEventArgs e)
        {
            LoadHotkeyControls();
        }

        private void LoadHotkeyControls()
        {
            HotkeysContainer.Children.Clear();

            foreach (PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
            {
                var control = new HotkeySettingControl();
                control.Initialize(action);
                HotkeysContainer.Children.Add(control);
            }

            Debug.WriteLine("[HotkeySettingsExpander] Loaded hotkey controls");
        }

        private async void DisableAllButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Отключить все горячие клавиши?",
                Content = "Все горячие клавиши для управления воспроизведением будут отключены. Вы сможете включить их снова в любой момент.",
                PrimaryButtonText = "Отключить",
                CloseButtonText = "Отмена",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (HotkeyService.Instance != null)
                {
                    foreach (PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
                    {
                        var binding = HotkeyService.Instance.GetBinding(action);
                        if (binding != null)
                        {
                            HotkeyService.Instance.UpdateBinding(action, binding.KeyCode, binding.Modifiers, false);
                        }
                    }
                    LoadHotkeyControls();
                }
            }
        }

        private async void ResetAllButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Сбросить все горячие клавиши?",
                Content = "Все горячие клавиши будут сброшены к стандартным комбинациям.",
                PrimaryButtonText = "Сбросить",
                CloseButtonText = "Отмена",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                HotkeyService.Instance?.ResetToDefaults();
                LoadHotkeyControls();
            }
        }

        private async void EnableAllButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Включить все горячие клавиши?",
                Content = "Все горячие клавиши для управления воспроизведением будут включены.",
                PrimaryButtonText = "Включить",
                CloseButtonText = "Отмена",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (HotkeyService.Instance != null)
                {
                    foreach (PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
                    {
                        var binding = HotkeyService.Instance.GetBinding(action);
                        if (binding != null)
                        {
                            HotkeyService.Instance.UpdateBinding(action, binding.KeyCode, binding.Modifiers, true);
                        }
                    }
                    LoadHotkeyControls();
                }
            }
        }
    }
}