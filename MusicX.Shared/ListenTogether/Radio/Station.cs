using ProtoBuf;
using System.Text.Json.Serialization;

namespace MusicX.Shared.ListenTogether.Radio
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
    public class Station
    {
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("cover")]
        public string Cover { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("owner")]
        public StationOwner Owner { get; set; }
    }
}
