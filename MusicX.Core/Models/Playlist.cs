using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models
{
    public class Playlist
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }

        public string? OwnerName { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("followers")]
        public long Followers { get; set; }

        [JsonProperty("plays")]
        public long Plays { get; set; }

        [JsonProperty("create_time")]
        public long CreateTime { get; set; }

        [JsonProperty("update_time")]
        public long UpdateTime { get; set; }

        [JsonProperty("genres")]
        public List<Genre> Genres { get; set; } = new List<Genre>();

        [JsonProperty("is_following")]
        public bool IsFollowing { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("original")]
        public Original? Original { get; set; }

        [JsonProperty("followed")]
        public Followed? Followed { get; set; }

        [JsonProperty("photo")]
        public Photo? Photo { get; set; }

        [JsonProperty("permissions")]
        public Permissions Permissions { get; set; }

        [JsonProperty("subtitle_badge")]
        public bool SubtitleBadge { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("play_button")]
        public bool PlayButton { get; set; }

        [JsonProperty("access_key")]
        public string AccessKey { get; set; }

        [JsonProperty("is_explicit")]
        public bool IsExplicit { get; set; }

        [JsonProperty("main_artists")]
        public List<MainArtist> MainArtists { get; set; } = new List<MainArtist>();

        [JsonProperty("album_type")]
        public string AlbumType { get; set; }

        [JsonProperty("audios")]
        public List<Audio> Audios { get; set; } = new List<Audio>();

        public string Cover
        {
            get
            {
                if (Photo == null) return null;
                if (Photo.Photo270 != null) return Photo.Photo270;
                if (Photo.Photo135 != null) return Photo.Photo135;
                if (Photo.Photo300 != null) return Photo.Photo300;
                if (Photo.Photo600 != null) return Photo.Photo600;
                if (Photo.Photo1200 != null) return Photo.Photo1200;
                if (Photo.Photo68 != null) return Photo.Photo68;
                return null;


            }
        }
    }
}
