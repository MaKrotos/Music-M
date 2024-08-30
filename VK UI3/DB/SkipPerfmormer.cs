using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.DB
{
    public class SkipPerfmormer
    {
        public SkipPerfmormer()
        {
        }
        public SkipPerfmormer(string performer)
        {
            Performer = performer;
        }

        [PrimaryKey]
        [AutoIncrement]
       public long id { get; set; }
       public string Performer { get; set; }

    }

    class SkipPerformerDB
    {
        public void createSkip(string performer) {
            DatabaseHandler.getConnect().Insert(new SkipPerfmormer(performer));
        }
        public void DelPerformer(string performer) {
            DatabaseHandler.getConnect().Table<SkipPerfmormer>().Where(perform => perform.Performer.Equals(performer)).Delete();
        
        }

        public bool skipIsSet(string performer)
        {
            if (DatabaseHandler.getConnect().Table<SkipPerfmormer>().Where(perform => perform.Performer.Equals(performer)).Count() == 0)
                return false;
            return true;
        }
    
    }
}
