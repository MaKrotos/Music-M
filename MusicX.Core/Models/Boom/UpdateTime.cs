using Newtonsoft.Json;

namespace MusicX.Core.Models.Boom
{
    public class UpdateTime
    {
        [JsonProperty("artists")]
        public int Artists { get; set; }

        [JsonProperty("albums")]
        public int Albums { get; set; }

        [JsonProperty("celebrityPlaylists")]
        public int CelebrityPlaylists { get; set; }

        [JsonProperty("audioUpdatesFeed")]
        public int AudioUpdatesFeed { get; set; }

        [JsonProperty("playlists")]
        public int Playlists { get; set; }
    }
}
