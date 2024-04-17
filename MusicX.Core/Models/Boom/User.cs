using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class User
    {
        [JsonProperty("oauthId")]
        public string OauthId { get; set; }

        [JsonProperty("avatar")]
        public Avatar Avatar { get; set; }

        [JsonProperty("playlistSyncProgress")]
        public PlaylistSyncProgress PlaylistSyncProgress { get; set; }

        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("importMusicEnabled")]
        public bool ImportMusicEnabled { get; set; }

        [JsonProperty("oauthSource")]
        public string OauthSource { get; set; }

        [JsonProperty("shareHash")]
        public string ShareHash { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("isOnline")]
        public bool IsOnline { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("apiId")]
        public string ApiId { get; set; }

        [JsonProperty("billing")]
        public Billing Billing { get; set; }

        [JsonProperty("updateTime")]
        public UpdateTime UpdateTime { get; set; }

        [JsonProperty("hasFeed")]
        public bool HasFeed { get; set; }

        [JsonProperty("cover")]
        public Avatar Cover { get; set; }

        [JsonProperty("abExperiments")]
        public List<AbExperiment> AbExperiments { get; set; }

        [JsonProperty("counts")]
        public Counts Counts { get; set; }
    }
}
