using SQLite;
using System;
using System.IO;
using static VK_UI3.DB.AccountsDB;

namespace VK_UI3.DB
{
    public class DatabaseHandler
    {

        static SQLiteConnection _db = null;




        public static SQLiteConnection getConnect()
        {
            // Получаем имя пользователя и номер материнской платы
            var userName = Environment.UserName;
            // var motherboardSerialNumber = GetMotherboardSerialNumber();

            // Путь к папке AppData текущего пользователя
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Путь к папке базы данных в папке AppData
            var databaseFolderPath = Path.Combine(appDataPath, "VKMMKZ");

            // Проверяем, существует ли папка, и если нет, создаем ее
            if (!Directory.Exists(databaseFolderPath))
            {
                Directory.CreateDirectory(databaseFolderPath);
            }

            // Путь к файлу базы данных
            var databasePath = Path.Combine(databaseFolderPath, "dbqkorozydatabase.db");

            if (_db == null)
            {
                // Добавляем имя пользователя и номер материнской платы к паролю
                var key = "wtwtiojvnsldji352I*YUIBNK" + userName;

                var options = new SQLiteConnectionString(databasePath, true, key: key);

                try
                {
                    _db = new SQLiteConnection(options);
                    _db.CreateTable<Accounts>();
                    _db.CreateTable<PathTable>();
                    _db.CreateTable<SettingsTable>();
                    _db.CreateTable<SkipPerfmormer>();

                }
                catch (SQLiteException ex)
                {
                    if (ex.Message.Contains("file is not a database"))
                    {
                        // Если пароль не подходит, удаляем файл БД и создаем новый
                        File.Delete(databasePath);
                        _db = new SQLiteConnection(options);
                        _db.CreateTable<Accounts>();
                        _db.CreateTable<PathTable>();
                        _db.CreateTable<SettingsTable>();
                        _db.CreateTable<SkipPerfmormer>();
                    }
                    else
                    {
                        throw;
                    }
                }
            }



            return _db;
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
