using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Shared
{
    public class Version
    {
        public string Ver { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public bool IsBeta { get; set; }
        public string Changelog { get; set; }
    }
}
