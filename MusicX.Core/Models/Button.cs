using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Button
    {
        [JsonProperty("action")]
        public ActionButton Action { get; set; }

        [JsonProperty("section_id")]
        public string SectionId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("ref_items_count")]
        public int RefItemsCount { get; set; }

        [JsonProperty("ref_layout_name")]
        public string RefLayoutName { get; set; }

        [JsonProperty("ref_data_type")]
        public string RefDataType { get; set; }

        [JsonProperty("block_id")]
        public string BlockId { get; set; }

        [JsonProperty("options")]
        public List<OptionButton> Options { get; set; } = new List<OptionButton>();

        [JsonProperty("artist_id")]
        public string ArtistId { get; set; }

        [JsonProperty("is_following")]
        public bool IsFollowing { get; set; }

        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }
    }
}
