using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Badge
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
