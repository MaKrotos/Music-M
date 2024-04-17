using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class TrackEvent
    {
        [JsonProperty("e")]
        public string Event { get; set; }

        [JsonProperty("uuid")]
        public int Uuid { get; set; }

    }
}
