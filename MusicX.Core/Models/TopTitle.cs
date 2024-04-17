using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class TopTitle
    {
        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

}
