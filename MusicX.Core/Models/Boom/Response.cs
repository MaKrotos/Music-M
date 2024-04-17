using Newtonsoft.Json;

namespace MusicX.Core.Models.Boom
{
    public class Response
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}
