using Newtonsoft.Json;

namespace MusicX.Core.Models;

public class MusicOwner
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("image")]
    public List<Image> Images { get; set; }
    
    [JsonProperty("subtitle")]
    public string Subtitle { get; set; }
    
    [JsonProperty("title")]
    public string Title { get; set; }
    
    [JsonProperty("url")]
    public string Url { get; set; }
}