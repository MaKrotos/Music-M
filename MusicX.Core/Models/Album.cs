using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Album
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }

        [JsonProperty("access_key")]
        public string AccessKey { get; set; }

        [JsonProperty("thumb")]
        public Photo Thumb { get; set; }

        public string? Cover
        {
            get
            {
                return Thumb.Photo270 ??
                    Thumb.Photo300 ??
                    Thumb.Photo600 ??
                    Thumb.Photo1200 ??
                    Thumb.Photo135 ??
                    Thumb.Photo68 ??
                    null;


            }
        }
    }
}
