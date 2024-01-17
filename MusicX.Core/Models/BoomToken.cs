using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class BoomToken
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("ttl")]
        public int Ttl { get; set; }

        [JsonProperty("photo_50")]
        public string Photo50 { get; set; }

        [JsonProperty("photo_100")]
        public string Photo100 { get; set; }

        [JsonProperty("photo_200")]
        public string Photo200 { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("weight")]
        public int Weight { get; set; }

        [JsonProperty("user_hash")]
        public string UserHash { get; set; }

        [JsonProperty("app_service_id")]
        public int AppServiceId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        public string Uuid { get; set; }
    }
}
