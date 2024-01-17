using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models
{
    public class Lyrics
    {
        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("lyrics")]
        public LyricsInfo LyricsInfo { get; set; }

        [JsonProperty("credits")]
        public string Credits { get; set; }
    }
}
