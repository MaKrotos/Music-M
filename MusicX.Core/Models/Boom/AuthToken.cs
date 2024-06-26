﻿using Newtonsoft.Json;

namespace MusicX.Core.Models.Boom
{
    public class AuthToken
    {
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
