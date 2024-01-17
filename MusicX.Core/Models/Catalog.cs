using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
