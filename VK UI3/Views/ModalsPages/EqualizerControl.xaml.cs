using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Services.Player.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using VK_UI3.DB;
using System.Text.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages;

public sealed partial class EqualizerControl : UserControl
{
    public event EventHandler EqualizerChanged;

    public EqualizerControl()
    {
        this.InitializeComponent();
        LoadSettings();
    }

    private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        EqualizerChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetEqualizerFilterString()
    {
        return $"equalizer=f=30:width_type=h:width=20:g={Slider30Hz.Value}," +
               $"equalizer=f=60:width_type=h:width=40:g={Slider60Hz.Value}," +
               $"equalizer=f=80:width_type=h:width=60:g={Slider80Hz.Value}," +
               $"equalizer=f=120:width_type=h:width=80:g={Slider120Hz.Value}," +
               $"equalizer=f=250:width_type=h:width=100:g={Slider250Hz.Value}," +
               $"equalizer=f=400:width_type=h:width=150:g={Slider400Hz.Value}," +
               $"equalizer=f=600:width_type=h:width=200:g={Slider600Hz.Value}," +
               $"equalizer=f=1000:width_type=h:width=300:g={Slider1kHz.Value}," +
               $"equalizer=f=2500:width_type=h:width=600:g={Slider2_5kHz.Value}," +
               $"equalizer=f=4000:width_type=h:width=1000:g={Slider4kHz.Value}," +
               $"equalizer=f=6000:width_type=h:width=1500:g={Slider6kHz.Value}," +
               $"equalizer=f=8000:width_type=h:width=2000:g={Slider8kHz.Value}," +
               $"equalizer=f=10000:width_type=h:width=3000:g={Slider10kHz.Value}," +
               $"equalizer=f=12000:width_type=h:width=4000:g={Slider12kHz.Value}," +
               $"equalizer=f=16000:width_type=h:width=5000:g={Slider16kHz.Value}";
    }

    public AudioEqualizer getEqualizes()
    {
        // Создаем эквалайзер и применяем текущие настройки
        var eq = new AudioEqualizer();

        // Применяем текущие значения слайдеров к эквалайзеру
        eq.SetBandGain("30Hz", (float)Slider30Hz.Value);
        eq.SetBandGain("60Hz", (float)Slider60Hz.Value);
        eq.SetBandGain("80Hz", (float)Slider80Hz.Value);
        eq.SetBandGain("120Hz", (float)Slider120Hz.Value);
        eq.SetBandGain("250Hz", (float)Slider250Hz.Value);
        eq.SetBandGain("400Hz", (float)Slider400Hz.Value);
        eq.SetBandGain("600Hz", (float)Slider600Hz.Value);
        eq.SetBandGain("1kHz", (float)Slider1kHz.Value);
        eq.SetBandGain("2.5kHz", (float)Slider2_5kHz.Value);
        eq.SetBandGain("4kHz", (float)Slider4kHz.Value);
        eq.SetBandGain("6kHz", (float)Slider6kHz.Value);
        eq.SetBandGain("8kHz", (float)Slider8kHz.Value);
        eq.SetBandGain("10kHz", (float)Slider10kHz.Value);
        eq.SetBandGain("12kHz", (float)Slider12kHz.Value);
        eq.SetBandGain("16kHz", (float)Slider16kHz.Value);

        return eq;
    }

    public void ApplyPreset(string presetName)
    {
        ResetAll();

        switch (presetName)
        {
            case "Flat":
                // Все значения уже сброшены в 0
                break;

            case "Pop":
                Slider60Hz.Value = 4;
                Slider80Hz.Value = 3;
                Slider120Hz.Value = 2;
                Slider10kHz.Value = 3;
                Slider12kHz.Value = 2;
                break;

            case "Rock":
                Slider80Hz.Value = 3;
                Slider250Hz.Value = 2;
                Slider1kHz.Value = 3;
                Slider2_5kHz.Value = 2;
                Slider4kHz.Value = 1;
                break;

            case "Classical":
                Slider60Hz.Value = 1;
                Slider600Hz.Value = -1;
                Slider10kHz.Value = 2;
                Slider16kHz.Value = 3;
                break;

            case "Jazz":
                Slider80Hz.Value = 2;
                Slider250Hz.Value = 1;
                Slider600Hz.Value = 2;
                Slider1kHz.Value = 1;
                Slider2_5kHz.Value = 2;
                Slider6kHz.Value = 1;
                break;

            case "Electronic":
                Slider60Hz.Value = 5;
                Slider120Hz.Value = 3;
                Slider250Hz.Value = 1;
                Slider4kHz.Value = 2;
                Slider10kHz.Value = 4;
                Slider12kHz.Value = 3;
                break;

            case "HipHop":
                Slider60Hz.Value = 5;
                Slider80Hz.Value = 4;
                Slider120Hz.Value = 3;
                Slider250Hz.Value = 1;
                Slider1kHz.Value = -1;
                Slider6kHz.Value = 2;
                break;

            case "Vocal":
                Slider250Hz.Value = -2;
                Slider600Hz.Value = 2;
                Slider1kHz.Value = 3;
                Slider2_5kHz.Value = 4;
                Slider4kHz.Value = 2;
                Slider6kHz.Value = 1;
                break;

            case "BassBoost":
                Slider30Hz.Value = 6;
                Slider60Hz.Value = 5;
                Slider80Hz.Value = 4;
                Slider120Hz.Value = 3;
                Slider250Hz.Value = 1;
                Slider1kHz.Value = -2;
                break;

            case "TrebleBoost":
                Slider1kHz.Value = -1;
                Slider2_5kHz.Value = 2;
                Slider4kHz.Value = 3;
                Slider6kHz.Value = 4;
                Slider10kHz.Value = 5;
                Slider16kHz.Value = 4;
                break;

            case "Acoustic":
                Slider80Hz.Value = 2;
                Slider250Hz.Value = 1;
                Slider600Hz.Value = 3;
                Slider1kHz.Value = 2;
                Slider2_5kHz.Value = 1;
                Slider6kHz.Value = 2;
                Slider10kHz.Value = 1;
                break;

            case "Dance":
                Slider60Hz.Value = 4;
                Slider120Hz.Value = 3;
                Slider250Hz.Value = 1;
                Slider1kHz.Value = -1;
                Slider4kHz.Value = 2;
                Slider10kHz.Value = 3;
                Slider12kHz.Value = 2;
                break;

            case "R&B":
                Slider80Hz.Value = 3;
                Slider250Hz.Value = 2;
                Slider600Hz.Value = 1;
                Slider1kHz.Value = 2;
                Slider2_5kHz.Value = 1;
                Slider6kHz.Value = 2;
                break;

            default:
                // Неизвестный пресет - оставляем плоский
                break;
        }
    }

    public void ResetAll()
    {
        Slider30Hz.Value = 0;
        Slider60Hz.Value = 0;
        Slider80Hz.Value = 0;
        Slider120Hz.Value = 0;
        Slider250Hz.Value = 0;
        Slider400Hz.Value = 0;
        Slider600Hz.Value = 0;
        Slider1kHz.Value = 0;
        Slider2_5kHz.Value = 0;
        Slider4kHz.Value = 0;
        Slider6kHz.Value = 0;
        Slider8kHz.Value = 0;
        Slider10kHz.Value = 0;
        Slider12kHz.Value = 0;
        Slider16kHz.Value = 0;
    }

    public void SaveSettings()
    {
        var values = new Dictionary<string, double>
        {
            {"30Hz", Slider30Hz.Value},
            {"60Hz", Slider60Hz.Value},
            {"80Hz", Slider80Hz.Value},
            {"120Hz", Slider120Hz.Value},
            {"250Hz", Slider250Hz.Value},
            {"400Hz", Slider400Hz.Value},
            {"600Hz", Slider600Hz.Value},
            {"1kHz", Slider1kHz.Value},
            {"2.5kHz", Slider2_5kHz.Value},
            {"4kHz", Slider4kHz.Value},
            {"6kHz", Slider6kHz.Value},
            {"8kHz", Slider8kHz.Value},
            {"10kHz", Slider10kHz.Value},
            {"12kHz", Slider12kHz.Value},
            {"16kHz", Slider16kHz.Value}
        };
        string json = JsonSerializer.Serialize(values);
        SettingsTable.SetSetting("Equalizer_Settings", json);
    }

    public void LoadSettings()
    {
        var setting = SettingsTable.GetSetting("Equalizer_Settings");
        if (setting == null) return;
        try
        {
            var values = JsonSerializer.Deserialize<Dictionary<string, double>>(setting.settingValue);
            if (values == null) return;
            if (values.TryGetValue("30Hz", out var v)) Slider30Hz.Value = v;
            if (values.TryGetValue("60Hz", out v)) Slider60Hz.Value = v;
            if (values.TryGetValue("80Hz", out v)) Slider80Hz.Value = v;
            if (values.TryGetValue("120Hz", out v)) Slider120Hz.Value = v;
            if (values.TryGetValue("250Hz", out v)) Slider250Hz.Value = v;
            if (values.TryGetValue("400Hz", out v)) Slider400Hz.Value = v;
            if (values.TryGetValue("600Hz", out v)) Slider600Hz.Value = v;
            if (values.TryGetValue("1kHz", out v)) Slider1kHz.Value = v;
            if (values.TryGetValue("2.5kHz", out v)) Slider2_5kHz.Value = v;
            if (values.TryGetValue("4kHz", out v)) Slider4kHz.Value = v;
            if (values.TryGetValue("6kHz", out v)) Slider6kHz.Value = v;
            if (values.TryGetValue("8kHz", out v)) Slider8kHz.Value = v;
            if (values.TryGetValue("10kHz", out v)) Slider10kHz.Value = v;
            if (values.TryGetValue("12kHz", out v)) Slider12kHz.Value = v;
            if (values.TryGetValue("16kHz", out v)) Slider16kHz.Value = v;
        }
        catch { /* ignore errors */ }
    }

    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            isEnabled = value;
            UpdateControlsState();
        }
    }
    private bool isEnabled = true;

    private void UpdateControlsState()
    {
        var state = IsEnabled ? VisualStateManager.GoToState(this, "Normal", true)
                              : VisualStateManager.GoToState(this, "Disabled", true);

        foreach (var child in FindVisualChildren<Slider>(this))
        {
            child.IsEnabled = IsEnabled;
        }
    }

    private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                {
                    yield return (T)child;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }
}

public class DoubleToDbStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            return d.ToString("0");
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

