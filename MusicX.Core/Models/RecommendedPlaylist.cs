using Newtonsoft.Json;


namespace MusicX.Core.Models
{
    public class RecommendedPlaylist
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }

        [JsonProperty("percentage")]
        public string Percentage { get; set; }

        [JsonProperty("percentage_title")]
        public string PercentageTitle { get; set; }

        [JsonProperty("audios")]
        public List<string> AudiosIds { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("cover")]
        public string Cover { get; set; }

        public Playlist Playlist { get; set; }

        public List<Audio> Audios { get; set; } = new List<Audio>();
    }
}
