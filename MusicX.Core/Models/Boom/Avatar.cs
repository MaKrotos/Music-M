using Newtonsoft.Json;

namespace MusicX.Core.Models.Boom
{
    public class Avatar
    {
        [JsonProperty("avgColor")]
        public string AvgColor { get; set; }

        [JsonProperty("accentColor")]
        public string AccentColor { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
