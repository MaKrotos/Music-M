using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class Album
    {
        [JsonProperty("apiId")]
        public string ApiId { get; set; }

        [JsonProperty("cover")]
        public Avatar Cover { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
