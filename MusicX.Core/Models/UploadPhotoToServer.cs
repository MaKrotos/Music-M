using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class UploadPhotoToServer
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }
    }
}
