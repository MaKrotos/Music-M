using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class Artist
    {
        [JsonProperty("shareHash")]
        public string ShareHash { get; set; }

        [JsonProperty("updatedAt")]
        public int UpdatedAt { get; set; }

        [JsonProperty("umaTags")]
        public object UmaTags { get; set; }

        [JsonProperty("avatar")]
        public Avatar Avatar { get; set; }

        [JsonProperty("isLiked")]
        public bool IsLiked { get; set; }

        [JsonProperty("addedAt")]
        public int AddedAt { get; set; }

        [JsonProperty("isAutoGenCover")]
        public bool IsAutoGenCover { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("counts")]
        public CountsArtist Counts { get; set; }

        [JsonProperty("apiId")]
        public string ApiId { get; set; }

        [JsonProperty("relevantArtistsNames")]
        public List<string> RelevantArtistsNames { get; set; }

        [JsonProperty("isRadioCapable")]
        public bool IsRadioCapable { get; set; }
    }
}
