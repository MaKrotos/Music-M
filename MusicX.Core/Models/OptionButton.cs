using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models
{
    public class OptionButton
    {
        [JsonProperty("replacement_id")]
        public string ReplacementId { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("selected")]
        public int Selected { get; set; }
    }
}
