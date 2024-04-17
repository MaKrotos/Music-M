using Newtonsoft.Json;

namespace MusicX.Core
{
    public class UploadPlaylistCoverServerResult
    {
        [JsonProperty("response")]
        public UploadPlaylistCoverServer Response { get; set; }
    }

    public class UploadPlaylistCoverServer
    {
        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }
    }
}
