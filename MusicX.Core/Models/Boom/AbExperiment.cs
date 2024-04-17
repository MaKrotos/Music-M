using Newtonsoft.Json;

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
