using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class PodcastSliderItem
    {
        [JsonProperty("item_id")]
        public string ItemId { get; set; }

        [JsonProperty("slider_type")]
        public string SliderType { get; set; }

        [JsonProperty("episode")]
        public PodcastEpisode Episode { get; set; }
    }
}
