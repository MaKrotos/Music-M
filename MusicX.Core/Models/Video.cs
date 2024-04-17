using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Video
    {
        [JsonProperty("is_explicit")]
        public int IsExplicit { get; set; }

        [JsonProperty("main_artists")]
        public List<MainArtist> MainArtists { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("release_date")]
        public int ReleaseDate { get; set; }

        [JsonProperty("genres")]
        public List<Genre> Genres { get; set; }

        [JsonProperty("can_comment")]
        public int CanComment { get; set; }

        [JsonProperty("can_like")]
        public int CanLike { get; set; }

        [JsonProperty("can_repost")]
        public int CanRepost { get; set; }

        [JsonProperty("can_subscribe")]
        public int CanSubscribe { get; set; }

        [JsonProperty("can_add_to_faves")]
        public int CanAddToFaves { get; set; }

        [JsonProperty("can_add")]
        public int CanAdd { get; set; }

        [JsonProperty("can_download")]
        public int CanDownload { get; set; }

        [JsonProperty("date")]
        public int Date { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("image")]
        public List<Image> Image { get; set; }

        [JsonProperty("first_frame")]
        public List<FirstFrame> FirstFrame { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("ov_id")]
        public string OvId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("is_favorite")]
        public bool IsFavorite { get; set; }

        [JsonProperty("player")]
        public string Player { get; set; }

        [JsonProperty("added")]
        public int Added { get; set; }

        [JsonProperty("is_subscribed")]
        public int IsSubscribed { get; set; }

        [JsonProperty("track_code")]
        public string TrackCode { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("views")]
        public int Views { get; set; }


        [JsonProperty("uv_stats_place")]
        public string UvStatsPlace { get; set; }
    }

    public class FirstFrame
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }
}
