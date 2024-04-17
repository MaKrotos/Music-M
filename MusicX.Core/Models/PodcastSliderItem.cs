using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
