using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models
{
    public class UploadPlaylistCoverResult
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }
    }
}
