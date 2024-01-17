using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class LyricsTimestamp
    {
        [JsonProperty("begin")]
        public int Begin { get; set; }

        [JsonProperty("end")]
        public int End { get; set; }

        [JsonProperty("line")]
        public string Line { get; set; }
    }
}
