using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupLib
{
    public class ReleaseInfo
    {
        public string Version { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public int Size { get; set; }
        public string CertificateUrl { get; set; }
        public string MsixUrl { get; set; }
        public bool IsNewVersionAvailable { get; set; }
    }
}
