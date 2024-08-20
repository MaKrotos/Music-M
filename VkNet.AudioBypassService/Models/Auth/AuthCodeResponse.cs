using System.Text.Json.Serialization;
using System;

namespace VkNet.AudioBypassService.Models.Auth;

[Serializable]
public class AuthCodeResponse
{
    [JsonPropertyName("auth_code")]
    public string AuthCode { get; set; }

    [JsonPropertyName("auth_hash")]
    public string AuthHash { get; set; }

    [JsonPropertyName("auth_id")]
    public string AuthId { get; set; }

    [JsonPropertyName("auth_url")]
    public string AuthUrl { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    public string Token { get; set; }

    public Object image { get; set; }

   
}