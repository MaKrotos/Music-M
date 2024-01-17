using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class Avatar
    {
        [JsonProperty("avgColor")]
        public string AvgColor { get; set; }

        [JsonProperty("accentColor")]
        public string AccentColor { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
