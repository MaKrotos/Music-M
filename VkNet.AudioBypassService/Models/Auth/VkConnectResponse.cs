using System.Text.Json.Serialization;

namespace VkNet.AudioBypassService.Models.Auth;

public record VkConnectResponse(
    [property: JsonPropertyName("expires_in")] long ExpiresIn,
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);