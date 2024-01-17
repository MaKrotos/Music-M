using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class Counts
    {
        [JsonProperty("album")]
        public int Album { get; set; }

        [JsonProperty("artist")]
        public int Artist { get; set; }

        [JsonProperty("track")]
        public int Track { get; set; }

        [JsonProperty("friend")]
        public int Friend { get; set; }

        [JsonProperty("playlist")]
        public int Playlist { get; set; }
    }
}
