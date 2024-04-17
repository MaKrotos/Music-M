using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class PlayPlaylistTrackEvent : TrackEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
