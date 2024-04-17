using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class PlaylistSyncProgress
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("ready")]
        public int Ready { get; set; }
    }
}
