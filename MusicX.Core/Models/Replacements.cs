using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Replacement
    {
        [JsonProperty("from_block_ids")]
        public List<string> FromBlockIds { get; set; }

        [JsonProperty("to_blocks")]
        public List<Block> ToBlocks { get; set; }
    }

    public class Replacements
    {
        [JsonProperty("replacements")]
        public List<Replacement> ReplacementsModels { get; set; }

        [JsonProperty("new_next_from")]
        public string NewNextFrom { get; set; }
    }
}
