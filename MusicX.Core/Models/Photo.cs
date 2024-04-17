using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Photo
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("photo_34")]
        public string Photo34 { get; set; }

        [JsonProperty("photo_68")]
        public string Photo68 { get; set; }

        [JsonProperty("photo_135")]
        public string Photo135 { get; set; }

        [JsonProperty("photo_270")]
        public string Photo270 { get; set; }

        [JsonProperty("photo_300")]
        public string Photo300 { get; set; }

        [JsonProperty("photo_600")]
        public string Photo600 { get; set; }

        [JsonProperty("photo_1200")]
        public string Photo1200 { get; set; }
    }
}
