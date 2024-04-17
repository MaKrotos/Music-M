using Newtonsoft.Json;

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
