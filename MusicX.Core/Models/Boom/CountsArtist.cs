using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class CountsArtist
    {
        [JsonProperty("newTrack")]
        public int NewTrack { get; set; }

        [JsonProperty("play")]
        public int Play { get; set; }
    }
}
