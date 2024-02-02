using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.DB;

namespace vkPosterBot.DB
{
    public class SettingsTable
    {
        [PrimaryKey]
        [AutoIncrement]
        public long id { get; set; }

        public string settingName { get; set; }

        public string settingValue { get; set; }

        public SettingsTable(string settingName, string settingValue)
        {
            this.settingName = settingName;
            this.settingValue = settingValue;
        }

        public SettingsTable() { }


        public static SettingsTable? GetSetting(string settingName) 
        {
            var setting = DatabaseHandler.getConnect().Query<SettingsTable>("Select * FROM SettingsTable WHERE settingName = '"+ settingName+"'");

            if (setting.Count == 0) return null;
            return setting[0];
        }

        public static void RemoveSetting(string settingName) 
        {
            DatabaseHandler.getConnect().Query<SettingsTable>("DELETE FROM SettingsTable WHERE settingName = '" + settingName + "'");

        }


        public static void SetSetting(string settingName, string value)
        {
           var setting = GetSetting(settingName);
            if (setting == null)
            {

                setting = new SettingsTable(settingName, value);
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

