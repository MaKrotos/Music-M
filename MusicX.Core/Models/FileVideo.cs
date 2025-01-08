
using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class FileVideo
    {
        [JsonExtensionData]
        public Dictionary<string, object> AdditionalData { get; set; }


        [JsonProperty("hls")]
        public string hls { get; set; }

        [JsonProperty("mp4_144")]
        public string Mp4_144 { get; set; }

        [JsonProperty("mp4_240")]
        public string Mp4_240 { get; set; }

        [JsonProperty("mp4_360")]
        public string Mp4_360 { get; set; }

        [JsonProperty("mp4_480")]
        public string Mp4_480 { get; set; }

        [JsonProperty("mp4_720")]
        public string Mp4_720 { get; set; }

        [JsonProperty("mp4_1080")]
        public string Mp4_1080 { get; set; }

        [JsonProperty("mp4_1440")]
        public string Mp4_1440 { get; set; }

        [JsonProperty("mp4_2160")]
        public string Mp4_2160 { get; set; }

        public Dictionary<string, string> Quality
        {
            get
            {
                var qualityDict = new Dictionary<string, string>();
                var properties = typeof(FileVideo).GetProperties();

                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(string) && !string.IsNullOrEmpty((string)property.GetValue(this)))
                    {
                        var key = property.Name.Replace("Mp4_", "") + "p";
                        qualityDict.Add(key, (string)property.GetValue(this));
                    }
                }

                if (qualityDict.Count > 0)
                {
                    return qualityDict;
                }
                else
                {
                    return null;
                }
            }
        }

    }

}