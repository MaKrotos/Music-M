using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models
{
    public class Section
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("blocks")]
        public List<Block> Blocks { get; set; } = new List<Block>();

        [JsonProperty("next_from")]
        public string NextFrom { get; set; }
    }
}
