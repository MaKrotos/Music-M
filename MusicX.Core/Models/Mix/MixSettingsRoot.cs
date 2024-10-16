using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Mix
{
    public class MixSettingsRoot
    {
        [JsonPropertyName("settings")]
        public MixSettings Settings { get; set; }
    }
}
