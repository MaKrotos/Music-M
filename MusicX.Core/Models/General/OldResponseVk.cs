using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.General
{
    public class OldResponseVk<T>
    {
        [JsonProperty("response")]
        public OldResponseData<T> Response { get; set; }

        [JsonProperty("error")]
        public ErrorVk Error { get; set; }
    }

    public class OldResponseData<T>
    {
        [JsonProperty("items")]
        public List<T> Items { get; set; }
    }
}
