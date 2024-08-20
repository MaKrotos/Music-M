using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace VkNet.AudioBypassService.Models.Auth;


[Serializable]
public class AuthCheckResponse
{
    [JsonPropertyName("status")]
    public Statuses Status { get; set; }

    [JsonPropertyName("expires_in")]
    public int? expires_in { get; set; }

    [JsonPropertyName("user_id")]
    public long? userID { get; set; }

    [JsonPropertyName("super_app_token")]
    public string? super_app_token { get; set; }


    [JsonPropertyName("need_password")]
    public bool? need_password { get; set; }

    [JsonPropertyName("is_partial")]
    public bool? is_partial { get; set; }


    [JsonPropertyName("provider_app_id")]
    public int? provider_app_id { get; set; }


    [JsonExtensionData]
    public Dictionary<string, object> AdditionalData { get; set; }
}
public enum Statuses
{
    Continue,
    ConfirmOnPhone,
    Ok,
    Expired = 4,
    Loading = 200
}

