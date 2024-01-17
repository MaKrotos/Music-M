using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models
{
    public class TopTitle
    {
        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

}
