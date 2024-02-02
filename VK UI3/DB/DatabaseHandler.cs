
using Microsoft.UI.Xaml;
using SQLite;
using System;
using System.IO;
using VkNet.Enums.Filters;
using vkPosterBot.DB;
using static VK_UI3.DB.AccountsDB;
using System.Management;

namespace VK_UI3.DB
{
    public class DatabaseHandler
    {

        static SQLiteConnection _db= null;




        public static SQLiteConnection getConnect()
        {
            // РџРѕР»СѓС‡Р°РµРј РёРјСЏ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ Рё РЅРѕРјРµСЂ РјР°С‚РµСЂРёРЅСЃРєРѕР№ РїР»Р°С‚С‹
            var userName = Environment.UserName;
           // var motherboardSerialNumber = GetMotherboardSerialNumber();

            // РџСѓС‚СЊ Рє РїР°РїРєРµ AppData С‚РµРєСѓС‰РµРіРѕ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // РџСѓС‚СЊ Рє РїР°РїРєРµ Р±Р°Р·С‹ РґР°РЅРЅС‹С… РІ РїР°РїРєРµ AppData
            var databaseFolderPath = Path.Combine(appDataPath, "VKMMKZ");

            // РџСЂРѕРІРµСЂСЏРµРј, СЃСѓС‰РµСЃС‚РІСѓРµС‚ Р»Рё РїР°РїРєР°, Рё РµСЃР»Рё РЅРµС‚, СЃРѕР·РґР°РµРј РµРµ
            if (!Directory.Exists(databaseFolderPath))
            {
                Directory.CreateDirectory(databaseFolderPath);
            }

            // РџСѓС‚СЊ Рє С„Р°Р№Р»Сѓ Р±Р°Р·С‹ РґР°РЅРЅС‹С…
            var databasePath = Path.Combine(databaseFolderPath, "dbqkorozydatabase.db");

            if (_db == null)
            {
                // Р”РѕР±Р°РІР»СЏРµРј РёРјСЏ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ Рё РЅРѕРјРµСЂ РјР°С‚РµСЂРёРЅСЃРєРѕР№ РїР»Р°С‚С‹ Рє РїР°СЂРѕР»СЋ
                var key = "wtwtiojvnsldji352I*YUIBNK" + userName;

                var options = new SQLiteConnectionString(databasePath, true, key: key);

                try
                {
                    _db = new SQLiteConnection(options);
                    _db.CreateTable<Accounts>();
                }
                catch (SQLiteException ex)
                {
                    if (ex.Message.Contains("file is not a database"))
                    {
                        // Р•СЃР»Рё РїР°СЂРѕР»СЊ РЅРµ РїРѕРґС…РѕРґРёС‚, СѓРґР°Р»СЏРµРј С„Р°Р№Р» Р‘Р” Рё СЃРѕР·РґР°РµРј РЅРѕРІС‹Р№
                        File.Delete(databasePath);
                        _db = new SQLiteConnection(options);
                        _db.CreateTable<Accounts>();
                        _db.CreateTable<SettingsTable>();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            _db.CreateTable<SettingsTable>();

            return _db;
        }



        private static string GetMotherboardSerialNumber()
        {
            string mbInfo = String.Empty;
            ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
            scope.Connect();
            ManagementObject wmiClass;
            wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());
            foreach (PropertyData propData in wmiClass.Properties)
            {
                if (propData.Name == "SerialNumber")
                    mbInfo = String.Format("{0,-25}{1}", propData.Name, Convert.ToString(propData.Value));
            }
            return mbInfo;
        }




        public static void clearBD()
        {

            /*
            getConnect().DropTable<Users>();
            getConnect().DropTable<UserDo>();
            getConnect().DropTable<chat>();
            getConnect().DropTable<RepMessDB>();
            getConnect().DropTable<Mess>();

            _db.CreateTable<Mess>();
            _db.CreateTable<Users>();
            _db.CreateTable<RepMessDB>();
            _db.CreateTable<UserDo>();
            _db.CreateTable<chat>();
            */

        }

    }
}

