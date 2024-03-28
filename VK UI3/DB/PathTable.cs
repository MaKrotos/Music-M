using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.DB
{
    public class PathTable
    {
        [PrimaryKey, AutoIncrement]
        public long id { get; set; }

        public string path { get; set; }

        public PathTable(string path)
        {
            this.path = path;
        }

        public PathTable() { }

        public static List<PathTable> GetAllPaths()
        {
            try
            {
                return DatabaseHandler.getConnect().Query<PathTable>("SELECT * FROM PathTable");
            }
            catch { return new List<PathTable>(); }
        }

        public static PathTable? GetPath(long id)
        {
            var path = DatabaseHandler.getConnect().Query<PathTable>("SELECT * FROM PathTable WHERE id = '" + id + "'");

            if (path.Count == 0) return null;
            return path[0];
        }

        public static void RemovePath(long id)
        {
            DatabaseHandler.getConnect().Query<PathTable>("DELETE FROM PathTable WHERE id = '" + id + "'");
        }

        public static void AddPath(string newPath)
        {
            var paths = GetAllPaths();
            if (paths.Any(p => p.path == newPath))
            {
                // Если путь уже существует, ничего не делаем
                return;
            }

            if (paths.Count >= 5)
            {
                // Удаляем самый старый путь (с наименьшим id)
                RemovePath(paths[0].id);
            }

            var path = new PathTable(newPath);
            DatabaseHandler.getConnect().Insert(path);
        }
    }

}
