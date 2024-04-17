using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
