using Newtonsoft.Json;

namespace MusicX.Core.Models.Boom
{
    public class Radio : IEquatable<Radio>
    {
        [JsonProperty("currentClusterId")]
        public string CurrentClusterId { get; set; }

        [JsonProperty("clusters")]
        public List<Cluster> Clusters { get; set; }

        [JsonProperty("tracks")]
        public List<Track> Tracks { get; set; }

        [JsonProperty("artist")]
        public Artist Artist { get; set; }

        [JsonProperty("tag")]
        public Tag Tag { get; set; }

        [JsonProperty("currentCluster")]
        public Cluster CurrentCluster { get; set; }

        [JsonProperty("apiId")]
        public string ApiId { get; set; }

        public bool Equals(Radio? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ApiId == other.ApiId;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Radio)obj);
        }

        public override int GetHashCode()
        {
            return ApiId.GetHashCode();
        }
    }
}
