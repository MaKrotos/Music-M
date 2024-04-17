using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class StopTrackEvent : TrackEvent
    {
        [JsonProperty("audio_id")]
        public string AudioId { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("shuffle")]
        public string Shuffle { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("playback_started_at")]
        public string PlaybackStartedAt { get; set; }

        [JsonProperty("track_code")]
        public string TrackCode { get; set; }

        [JsonProperty("repeat")]
        public string Repeat { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("playlist_id")]
        public string PlaylistId { get; set; }

        [JsonProperty("streaming_type")]
        public string StreamingType { get; set; }
    }
}
