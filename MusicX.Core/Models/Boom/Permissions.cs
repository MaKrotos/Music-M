using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class Permissions
    {
        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("permit")]
        public bool Permit { get; set; }
    }
}
