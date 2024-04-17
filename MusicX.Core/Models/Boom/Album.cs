using Newtonsoft.Json;

namespace MusicX.Core.Models.Boom
{
    public class Album
    {
        [JsonProperty("apiId")]
        public string ApiId { get; set; }

        [JsonProperty("cover")]
        public Avatar Cover { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
