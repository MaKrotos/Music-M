using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class Cluster
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("clusterId")]
        public string ClusterId { get; set; }

        [JsonProperty("artists")]
        public List<Artist> Artists { get; set; }

        [JsonProperty("cover")]
        public Avatar Cover { get; set; }

        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; }
    }
}
