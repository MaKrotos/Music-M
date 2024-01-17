using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models
{
    public class Link
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("image")]
        public List<Image> Image { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Meta
    {
        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("track_code")]
        public string TrackCode { get; set; }
    }
}
