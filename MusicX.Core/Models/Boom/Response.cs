using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class Response
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}
