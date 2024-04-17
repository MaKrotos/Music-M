using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models
{
    public class CuratorGroup
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("track_code")]
        public string TrackCode { get; set; }
    }
}
