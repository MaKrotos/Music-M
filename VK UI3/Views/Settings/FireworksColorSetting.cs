using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;
using Windows.UI;
using System;

namespace VK_UI3.Views.Settings
{
    public sealed class FireworksColorSetting : ColorPicker
    {
        public FireworksColorSetting()
        {
            // Настройки ColorPicker
            this.ColorSpectrumShape = ColorSpectrumShape.Box;
            this.IsMoreButtonVisible = false;
            this.IsColorSliderVisible = true;
            this.IsColorChannelTextInputVisible = true;
            this.IsHexInputVisible = true;
            this.IsAlphaEnabled = false;
            this.IsAlphaSliderVisible = true;
            this.IsAlphaTextInputVisible = true;

            this.Loaded += FireworksColorSetting_Loaded;
            this.ColorChanged += FireworksColorSetting_ColorChanged;

            // Добавляем свойства доступности для экранного диктера
            AutomationProperties.SetName(this, "Цвет фейерверков");
            AutomationProperties.SetHelpText(this, "Выберите цвет фейерверков. Если включен режим радуги, этот цвет игнорируется.");
        }

        private void FireworksColorSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                var setting = SettingsTable.GetSetting("fireworksColor");
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
                        // В случае ошибки используем цвет по умолчанию (красный)
                        this.Color = Color.FromArgb(255, 255, 50, 50);
                    }
                }
                else
                {
                    // Цвет по умолчанию (красный)
                    this.Color = Color.FromArgb(255, 255, 50, 50);
                }
            });
        }

        private void FireworksColorSetting_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            // Сохраняем цвет в формате #AARRGGBB
            string colorHex = $"#{args.NewColor.A:X2}{args.NewColor.R:X2}{args.NewColor.G:X2}{args.NewColor.B:X2}";
            SettingsTable.SetSetting("fireworksColor", colorHex);

            // Применяем цвет к фейерверкам
            if (MainWindow.mainWindow?.Fireworks != null)
                MainWindow.mainWindow.Fireworks.RocketColor = args.NewColor;
        }
    }
}