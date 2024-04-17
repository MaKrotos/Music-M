using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class Track
    {
        [JsonProperty("isExplicit")]
        public bool IsExplicit { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("apiId")]
        public string ApiId { get; set; }

        [JsonProperty("artist")]
        public Artist Artist { get; set; }

        [JsonProperty("artistDisplayName")]
        public string ArtistDisplayName { get; set; }

        [JsonProperty("shareHash")]
        public string ShareHash { get; set; }

        [JsonProperty("artists")]
        public List<Artist> Artists { get; set; }

        [JsonProperty("album")]
        public Album Album { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("isLiked")]
        public bool IsLiked { get; set; }

        [JsonProperty("isRestricted")]
        public bool IsRestricted { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("counts")]
        public Counts Counts { get; set; }

        [JsonProperty("permissions")]
        public Permissions Permissions { get; set; }

        [JsonProperty("isAdded")]
        public bool IsAdded { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isRadioCapable")]
        public bool IsRadioCapable { get; set; }

        [JsonProperty("isLegal")]
        public bool IsLegal { get; set; }

        [JsonProperty("cover")]
        public Avatar Cover { get; set; }
    }
}
