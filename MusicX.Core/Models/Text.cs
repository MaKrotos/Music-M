using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models
{
    public class Text
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Value { get; set; }

        [JsonProperty("collapsed_lines")]
        public int CollapsedLines { get; set; }
    }
}
