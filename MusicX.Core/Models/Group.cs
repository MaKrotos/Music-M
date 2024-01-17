using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models
{
    public class Group
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        [JsonProperty("is_closed")]
        public int IsClosed { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("is_admin")]
        public int IsAdmin { get; set; }

        [JsonProperty("is_member")]
        public int IsMember { get; set; }

        [JsonProperty("is_advertiser")]
        public int IsAdvertiser { get; set; }

        [JsonProperty("photo_50")]
        public string Photo50 { get; set; }

        [JsonProperty("photo_100")]
        public string Photo100 { get; set; }

        [JsonProperty("photo_200")]
        public string Photo200 { get; set; }

        [JsonProperty("member_status")]
        public int MemberStatus { get; set; }

        [JsonProperty("verified")]
        public int Verified { get; set; }

        [JsonProperty("members_count")]
        public int MembersCount { get; set; }

        [JsonProperty("activity")]
        public string Activity { get; set; }

        [JsonProperty("trending")]
        public int Trending { get; set; }
    }
}
