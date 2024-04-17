using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Cover
    {
        [JsonProperty("sizes")]
        public List<Image> Sizes { get; set; }
    }
}
