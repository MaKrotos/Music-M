using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models
{
    public class PlayPlaylistTrackEvent : TrackEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
