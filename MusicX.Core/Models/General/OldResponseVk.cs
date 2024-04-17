using Newtonsoft.Json;

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
