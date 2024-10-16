using Newtonsoft.Json;

namespace MusicX.Core.Models.Mix
{
    public class MixOption
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }


        [JsonProperty("icon")]
        public string IconUri { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }
    }
}
