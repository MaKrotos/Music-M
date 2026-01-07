using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Services.Player.Sources;
using System;
using VK_UI3.Controllers;
using VK_UI3.DB;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages;

public sealed partial class EqualizerControlPage : UserControl
{
    public event EventHandler ApplyClicked;
    public event EventHandler<bool> EnabledChanged;

    public EqualizerControlPage()
    {
        this.InitializeComponent();
        // Восстановление состояния включения эквалайзера
        var enabledSetting = SettingsTable.GetSetting("Equalizer_Enabled", "1");
        EnableSwitch.IsOn = enabledSetting.settingValue == "1";
    }

    private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PresetComboBox.SelectedItem is ComboBoxItem item && item.Tag is string presetName)
        {
            Equalizer.ApplyPreset(presetName);
        }
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (EnableSwitch.IsEnabled)
        {
            VK_UI3.Services.MediaPlayerService.Equalizer = Equalizer.getEqualizes();
        }
        else
        {
            VK_UI3.Services.MediaPlayerService.Equalizer = null;
        }
        Equalizer.SaveSettings();
        // Сохраняем состояние включения эквалайзера
        SettingsTable.SetSetting("Equalizer_Enabled", EnableSwitch.IsOn ? "1" : "0");
    }

    private void EnableSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        EnabledChanged?.Invoke(this, EnableSwitch.IsOn);
        Equalizer.IsEnabled = EnableSwitch.IsOn;
    }

    public string GetEqualizerSettings()
    {
        return EnableSwitch.IsOn ? Equalizer.GetEqualizerFilterString() : string.Empty;
    }

    public bool IsEqualizerEnabled => EnableSwitch.IsOn;
}

