using System.Text.Json.Serialization;

namespace VkNet.AudioBypassService.Models.Auth;

public record AuthRefreshAccessTokenDto(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);