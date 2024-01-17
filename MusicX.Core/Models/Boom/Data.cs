using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class Data
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("artists")]
        public List<Artist> Artists { get; set; }

        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; }

        [JsonProperty("radio")]
        public Radio Radio { get; set; }

        [JsonProperty("tracks")]
        public List<Track> Tracks { get; set; }

    }
}
