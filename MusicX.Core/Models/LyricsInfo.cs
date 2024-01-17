using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class LyricsInfo
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("timestamps")]
        public List<LyricsTimestamp> Timestamps { get; set; }

        [JsonProperty("text")]
        public List<string> Text { get; set; }
    }
}
