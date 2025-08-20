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
        public Dictionary<PackageType, PackageAsset> Assets { get; set; } = new Dictionary<PackageType, PackageAsset>();
        public string CertificateUrl { get; set; }
        public bool IsNewVersionAvailable { get; set; }
    }

    public class PackageAsset
    {
        public string Url { get; set; }
        public int Size { get; set; }
        public string Architecture { get; set; }
    }
}
