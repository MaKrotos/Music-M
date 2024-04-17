using Newtonsoft.Json;

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
