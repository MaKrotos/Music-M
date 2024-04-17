using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Suggestion
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("context")]
        public string Context { get; set; }
    }
}
