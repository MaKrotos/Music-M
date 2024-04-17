using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Artist
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("is_album_cover")]
        public bool IsAlbumCover { get; set; }

        [JsonProperty("photo")]
        public List<Image> Photo { get; set; }

        [JsonProperty("is_followed")]
        public bool IsFollowed { get; set; }

        [JsonProperty("can_follow")]
        public bool CanFollow { get; set; }
    }
}