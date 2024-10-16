using Newtonsoft.Json;

namespace MusicX.Core.Models.Mix
{
    public class MixSettings
    {
        [Newtonsoft.Json.JsonExtensionData]
        public Dictionary<string, object> AdditionalData { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("mix_categories")]
        public List<MixCategory> Categories { get; set; }
    }
}
