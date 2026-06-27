using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;
using Windows.UI;
using System;
using System.Threading.Tasks;

namespace VK_UI3.Views.Settings
{
    public sealed class ConfettiColorSetting : ColorPicker
    {
        public ConfettiColorSetting()
        {
            // Настройки ColorPicker согласно указанным параметрам
            this.ColorSpectrumShape = ColorSpectrumShape.Box;
            this.IsMoreButtonVisible = false;
            this.IsColorSliderVisible = true;
            this.IsColorChannelTextInputVisible = true;
            this.IsHexInputVisible = true;
            this.IsAlphaEnabled = false;
            this.IsAlphaSliderVisible = true;
            this.IsAlphaTextInputVisible = true;

            this.Loaded += ConfettiColorSetting_Loaded;
            this.ColorChanged += ConfettiColorSetting_ColorChanged;

            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Цвет конфетти");
            AutomationProperties.SetHelpText(this, "Выберите цвет конфетти. Если включен режим случайных цветов, этот цвет игнорируется.");
        }

        private void ConfettiColorSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var setting = SettingsTable.GetSetting("confettiColor");
                if (setting != null && !string.IsNullOrEmpty(setting.settingValue))
                {
                    try
                    {
                        // Ожидаемый формат #AARRGGBB или #RRGGBB
                        string colorStr = setting.settingValue.Trim();
                        if (colorStr.StartsWith("#"))
                        {
                            // Пропускаем # и парсим
                            colorStr = colorStr.Substring(1);
                            if (colorStr.Length == 6)
                            {
                                // RRGGBB, добавляем альфа FF
                                colorStr = "FF" + colorStr;
                            }
                            if (colorStr.Length == 8)
                            {
                                byte a = Convert.ToByte(colorStr.Substring(0, 2), 16);
                                byte r = Convert.ToByte(colorStr.Substring(2, 2), 16);
                                byte g = Convert.ToByte(colorStr.Substring(4, 2), 16);
                                byte b = Convert.ToByte(colorStr.Substring(6, 2), 16);
                                this.Color = Color.FromArgb(a, r, g, b);
                            }
                        }
                    }
                    catch
                    {
                        // В случае ошибки используем цвет по умолчанию
                        this.Color = Color.FromArgb(255, 255, 105, 180); // Розовый по умолчанию
                    }
                }
                else
                {
                    // Цвет по умолчанию
                    this.Color = Color.FromArgb(255, 255, 105, 180); // Розовый
                }
            });
        }

        private void ConfettiColorSetting_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            // Сохраняем цвет в формате #AARRGGBB асинхронно, чтобы не блокировать UI
            string colorHex = $"#{args.NewColor.A:X2}{args.NewColor.R:X2}{args.NewColor.G:X2}{args.NewColor.B:X2}";
            _ = Task.Run(() => SettingsTable.SetSetting("confettiColor", colorHex));
        }
    }
}