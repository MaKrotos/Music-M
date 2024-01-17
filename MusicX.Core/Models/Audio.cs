using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models
{
    public class Audio : IEquatable<Audio>
    {
        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        public string DurationString
        {
            get
            {
                var t = TimeSpan.FromSeconds(Duration);
                if (t.Hours > 0)
                    return t.ToString("h\\:mm\\:ss");
                return t.ToString("m\\:ss");
            }
        }

        [JsonProperty("access_key")]
        public string AccessKey { get; set; }

        [JsonProperty("is_explicit")]
        public bool IsExplicit { get; set; }

        [JsonProperty("is_focus_track")]
        public bool IsFocusTrack { get; set; }

        [JsonProperty("is_licensed")]
        public bool IsLicensed { get; set; }

        [JsonProperty("track_code")]
        public string TrackCode { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("date")]
        public long Date { get; set; }

        public DateTime DateTime => new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(Date).ToLocalTime();

        [JsonProperty("album")]
        public Album Album { get; set; }

        [JsonProperty("main_artists")]
        public List<MainArtist> MainArtists { get; set; }

        [JsonProperty("short_videos_allowed")]
        public bool ShortVideosAllowed { get; set; }

        [JsonProperty("stories_allowed")]
        public bool StoriesAllowed { get; set; }

        [JsonProperty("stories_cover_allowed")]
        public bool StoriesCoverAllowed { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("lyrics_id")]
        public long? LyricsId { get; set; }

        [JsonProperty("has_lyrics")]
        public bool? HasLyrics { get; set; }

        [JsonProperty("genre_id")]
        public long? GenreId { get; set; }

        public bool IsAvailable { get; set; } = true;


        [JsonProperty("featured_artists")]
        public List<MainArtist> FeaturedArtists { get; set; }

        public string ParentBlockId { get; set; }

        public string? DownloadPlaylistName { get; set; }
        public bool Equals(Audio? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Id == other.Id;
        }
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(Audio))
                return false;
            return Equals((Audio)obj);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
