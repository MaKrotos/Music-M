using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class AbExperiment
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("group")]
        public int Group { get; set; }
    }
}
