using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models
{
    public class Original
    {
        [JsonProperty("playlist_id")]
        public int PlaylistId { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("access_key")]
        public string AccessKey { get; set; }
    }
}
