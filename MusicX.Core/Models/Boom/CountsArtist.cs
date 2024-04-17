using Newtonsoft.Json;

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
