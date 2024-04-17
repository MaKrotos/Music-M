using Newtonsoft.Json;

namespace MusicX.Core.Models.Boom
{
    public class Tag
    {
        [JsonProperty("cover")]
        public Avatar Cover { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("apiId")]
        public string ApiId { get; set; }

        [JsonProperty("relevantArtistsNames")]
        public List<string> RelevantArtistsNames { get; set; }
    }
}
