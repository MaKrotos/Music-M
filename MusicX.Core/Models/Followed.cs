using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Followed
    {
        [JsonProperty("playlist_id")]
        public int PlaylistId { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }
    }
}
