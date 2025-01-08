using SQLite;
using VK_UI3.DB;

namespace VK_UI3.DB
{
    public class PlayTempTrackTable
    {
        [PrimaryKey]
        [AutoIncrement]
        public long id { get; set; }

        public string settingName { get; set; }

        public string settingValue { get; set; }

        public PlayTempTrackTable(string settingName, string settingValue)
        {
            this.settingName = settingName;
            this.settingValue = settingValue;
        }

        public PlayTempTrackTable() { }


        public static PlayTempTrackTable? GetSetting(string settingName)
        {
            var setting = DatabaseHandler.getConnect().Query<PlayTempTrackTable>("Select * FROM PlayTempTrackTable WHERE settingName = '" + settingName + "'");

            if (setting.Count == 0) return null;
            return setting[0];
        }

        public static void RemoveSetting(string settingName)
        {
            DatabaseHandler.getConnect().Query<PlayTempTrackTable>("DELETE FROM SettingsTable WHERE settingName = '" + settingName + "'");

        }


        public static void SetSetting(string settingName, string value)
        {
            var setting = GetSetting(settingName);
            if (setting == null)
            {

                setting = new PlayTempTrackTable(settingName, value);
                DatabaseHandler.getConnect().Insert(setting);


            }
            else
            {
                setting.settingValue = value;
                DatabaseHandler.getConnect().Update(setting);
            }

        }
    }
}
