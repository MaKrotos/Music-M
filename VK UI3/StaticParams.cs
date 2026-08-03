using System;
using Windows.Foundation.Metadata;

namespace VK_UI3
{
    internal class StaticParams
    {
        public static readonly string tokenStatSly = Environment.GetEnvironmentVariable("TOKEN_STAT_SLY");
    }

    public class VKMStatSly : StatSlyLib.StatSLY
    {
        public  static string Token { get; set; } = StaticParams.tokenStatSly;
        public VKMStatSly() : base(Token)
        {
        }

        /// <summary>
        /// Синхронизирует флаг IsEnabled с настройкой из БД.
        /// Вызывается при старте приложения и при изменении настройки.
        /// </summary>
        public static void SyncEnabledFromSettings()
        {
            try
            {
                var setting = DB.SettingsTable.GetSetting("StatSlyEnabled");
                if (setting == null)
                {
                    // По умолчанию статистика включена (opt-out)
                    IsEnabled = true;
                    DB.SettingsTable.SetSetting("StatSlyEnabled", "1");
                }
                else
                {
                    IsEnabled = setting.settingValue.Equals("1");
                }
            }
            catch
            {
                // При ошибке чтения — оставляем включённым
                IsEnabled = true;
            }
        }
    }
}
