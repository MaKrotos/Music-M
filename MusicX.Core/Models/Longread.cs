using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models
{
    public class Longread
    {
        [JsonProperty("access_key")]
        public string AccessKey { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("is_favorite")]
        public bool IsFavorite { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("owner_name")]
        public string OwnerName { get; set; }

        [JsonProperty("owner_photo")]
        public string OwnerPhoto { get; set; }

        [JsonProperty("photo")]
        public Cover Photo { get; set; }

        [JsonProperty("published_date")]
        public int PublishedDate { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("view_url")]
        public string ViewUrl { get; set; }

        [JsonProperty("views")]
        public int Views { get; set; }

        [JsonProperty("shares")]
        public int Shares { get; set; }

        [JsonProperty("can_report")]
        public bool CanReport { get; set; }
    }
}
