using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models
{
    public class TrackEvent
    {
        [JsonProperty("e")]
        public string Event { get; set; }

        [JsonProperty("uuid")]
        public int Uuid { get; set; }

    }
}
