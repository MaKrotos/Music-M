using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Catalog
    {
        [JsonProperty("default_section")]
        public string DefaultSection { get; set; }

        [JsonProperty("sections")]
        public List<Section> Sections { get; set; }
    }
}
