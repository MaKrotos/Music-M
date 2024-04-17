using Newtonsoft.Json;

namespace MusicX.Core.Models.Boom
{
    public class Permissions
    {
        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("permit")]
        public bool Permit { get; set; }
    }
}
