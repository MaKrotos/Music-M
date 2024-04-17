using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models
{
    public class Permissions
    {
        [JsonProperty("play")]
        public bool Play { get; set; }

        [JsonProperty("share")]
        public bool Share { get; set; }

        [JsonProperty("edit")]
        public bool Edit { get; set; }

        [JsonProperty("follow")]
        public bool Follow { get; set; }

        [JsonProperty("delete")]
        public bool Delete { get; set; }

        [JsonProperty("boom_download")]
        public bool BoomDownload { get; set; }
    }
}
