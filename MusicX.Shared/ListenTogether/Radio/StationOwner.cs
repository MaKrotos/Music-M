using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MusicX.Shared.ListenTogether.Radio
{
    public class StationOwner
    {
        [JsonPropertyName("vkId")]
        public long VkId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("photo")]
        public string Photo { get; set; }

        [JsonPropertyName("ownerCategory")]
        public OwnerCategory OwnerCategory { get; set; }
    }
}
