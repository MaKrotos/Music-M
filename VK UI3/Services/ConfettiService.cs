using DevWinUI;
using Microsoft.UI.Xaml;
using System;
using System.Globalization;
using System.Linq;
using VK_UI3.DB;
using Windows.Foundation;
using Windows.UI;

namespace VK_UI3.Services
{
    public sealed class ConfettiService
    {
        private readonly ConfettiCannon _confettiCannon;
        private DispatcherTimer _continuousTimer;
        private readonly Random _random = new();

        public ConfettiService(ConfettiCannon confettiCannon)
        {
            _confettiCannon = confettiCannon ?? throw new ArgumentNullException(nameof(confettiCannon));
        }

        public void Initialize()
        {
            var enabledSetting = SettingsTable.GetSetting("confettiEnabled");

            if (enabledSetting != null && enabledSetting.settingValue == "1")
            {
                // Небольшая задержка, чтобы ConfettiCannon успел инициализироваться (OnApplyTemplate)
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    StartContinuousMode();
                };
                timer.Start();
            }
        }

        public void StartContinuousMode()
        {
            StopContinuousMode();

            int intervalSeconds = GetIntSetting("confettiContinuousInterval", 8);
            if (intervalSeconds < 1) intervalSeconds = 1;

            _continuousTimer = new DispatcherTimer();
            _continuousTimer.Interval = TimeSpan.FromSeconds(intervalSeconds);
            _continuousTimer.Tick += (s, e) =>
            {
                FireConfetti();
            };
            _continuousTimer.Start();
        }

        public void StopContinuousMode()
        {
            if (_continuousTimer != null)
            {
                _continuousTimer.Stop();
                _continuousTimer = null;
            }
        }

        public void FireConfetti()
        {
            if (_confettiCannon == null)
                return;

            var presetSetting = SettingsTable.GetSetting("confettiPreset");
            string preset = presetSetting?.settingValue ?? "FireBasic";

            switch (preset)
            {
                case "FireBasic":
                    _confettiCannon.FireBasic();
                    break;
                case "FireRandomDirection":
                    _confettiCannon.FireRandomDirection();
                    break;
                case "FireRealistic":
                    _confettiCannon.FireRealistic();
                    break;
                case "FireFireworks":
                    _confettiCannon.FireFireworks();
                    break;
                case "FireStars":
                    _confettiCannon.FireStars();
                    break;
                case "FireSnow":
                    _confettiCannon.FireSnow();
                    break;
                case "FireSchoolPride":
                    _confettiCannon.FireSchoolPride();
                    break;
                case "Custom":
                default:
                    _confettiCannon.Fire(BuildOptions());
                    break;
            }
        }

        /// <summary>
        /// Запускает пресет FireSchoolPride с уменьшенной продолжительностью.
        /// Используется для приветственного конфетти при обновлении версии.
        /// </summary>
        public void FireSchoolPrideConfetti()
        {
            if (_confettiCannon == null)
                return;

            const int duration = 3 * 1000; // 3 секунды вместо 15

            DateTime animationEnd = DateTime.Now + TimeSpan.FromMilliseconds(duration);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            timer.Tick += (_, __) =>
            {
                double timeLeft = (animationEnd - DateTime.Now).TotalMilliseconds;
                if (timeLeft <= 0)
                {
                    timer.Stop();
                    return;
                }

                _confettiCannon.Fire(new ConfettiCannonOptions
                {
                    ParticleCount = 2,
                    Angle = 60,
                    Spread = 55,
                    Origin = new Point(0, 0.5),
                    Colors = new() { ParseColor("#bb0000"), ParseColor("#ffffff") },
                });
                _confettiCannon.Fire(new ConfettiCannonOptions
                {
                    ParticleCount = 2,
                    Angle = 120,
                    Spread = 55,
                    Origin = new Point(1, 0.5),
                    Colors = new() { ParseColor("#bb0000"), ParseColor("#ffffff") },
                });
            };
            timer.Start();
        }

        /// <summary>
        /// Создаёт ConfettiCannonOptions на основе всех настроек из БД.
        /// Используется для пресета "Custom".
        /// </summary>
        private ConfettiCannonOptions BuildOptions()
        {
            var options = new ConfettiCannonOptions();

            // Базовые параметры
            options.ParticleCount = GetIntSetting("confettiParticleCount", 100);
            options.Angle = GetDoubleSetting("confettiAngle", 90);
            options.Spread = GetDoubleSetting("confettiSpread", 45);
            options.StartVelocity = GetDoubleSetting("confettiStartVelocity", 45);
            options.Gravity = GetDoubleSetting("confettiGravity", 1);
            options.Drift = GetDoubleSetting("confettiDrift", 0);
            options.Ticks = GetIntSetting("confettiTicks", 200);
            options.Scalar = GetDoubleSetting("confettiScalar", 1);

            // Формы
            var shapesStr = SettingsTable.GetSetting("confettiShapes")?.settingValue ?? "square,circle";
            options.Shapes = shapesStr.Split(',', StringSplitOptions.TrimEntries).ToList();

            // Цвета
            var useRandomColors = SettingsTable.GetSetting("confettiUseRandomColors")?.settingValue == "1";
            if (!useRandomColors)
            {
                var colorStr = SettingsTable.GetSetting("confettiColor")?.settingValue;
                if (!string.IsNullOrEmpty(colorStr))
                {
                    options.Colors = new() { ParseColor(colorStr) };
                }
            }
            // Если useRandomColors == true, Colors не задаём — ConfettiCannon использует дефолтные

            // Origin (случайное место вылета)
            var useRandomOrigin = SettingsTable.GetSetting("confettiUseRandomOrigin")?.settingValue != "0";
            if (useRandomOrigin)
            {
                options.Origin = new Point(_random.NextDouble(), _random.NextDouble());
            }
            else
            {
                options.Origin = new Point(0.5, 0.6);
            }

            return options;
        }

        private int GetIntSetting(string key, int defaultValue)
        {
            var setting = SettingsTable.GetSetting(key);
            if (setting != null && int.TryParse(setting.settingValue, out int value))
                return value;
            return defaultValue;
        }

        private double GetDoubleSetting(string key, double defaultValue)
        {
            var setting = SettingsTable.GetSetting(key);
            if (setting != null && double.TryParse(setting.settingValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                return value;
            return defaultValue;
        }

        private Color ParseColor(string colorStr)
        {
            try
            {
                string str = colorStr.Trim();
                if (str.StartsWith("#"))
                {
                    str = str.Substring(1);
                    if (str.Length == 6)
                    {
                        str = "FF" + str;
                    }
                    if (str.Length == 8)
                    {
                        byte a = Convert.ToByte(str.Substring(0, 2), 16);
                        byte r = Convert.ToByte(str.Substring(2, 2), 16);
                        byte g = Convert.ToByte(str.Substring(4, 2), 16);
                        byte b = Convert.ToByte(str.Substring(6, 2), 16);
                        return Color.FromArgb(a, r, g, b);
                    }
                }
            }
            catch
            {
                // В случае ошибки используем цвет по умолчанию
            }
            return Color.FromArgb(255, 255, 105, 180); // Розовый по умолчанию
        }
    }
}