using Newtonsoft.Json;

namespace MusicX.Core.Models.Boom
{
    public class Status
    {
        [JsonProperty("promouser")]
        public bool Promouser { get; set; }

        [JsonProperty("resident")]
        public bool Resident { get; set; }

        [JsonProperty("editor")]
        public bool Editor { get; set; }

        [JsonProperty("moderator")]
        public bool Moderator { get; set; }

        [JsonProperty("serviceAccount")]
        public bool ServiceAccount { get; set; }
    }
}
