using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Shared
{
    public class CheckModel
    {
        public bool IsUpdated { get; set; }

        public List<Version> Versions { get; set; }

        public Version LastVersion { get; set; }

    }
}
