using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models
{
    public class Layout
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("is_editable")]
        public int? IsEditable { get; set; }

        [JsonProperty("top_title")]
        public TopTitle TopTitle { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }
    }
}
