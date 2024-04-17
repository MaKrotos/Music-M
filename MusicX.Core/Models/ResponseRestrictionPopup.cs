using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Icon
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class RestrictionPopupData
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("icons")]
        public List<Icon> Icons { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

}
