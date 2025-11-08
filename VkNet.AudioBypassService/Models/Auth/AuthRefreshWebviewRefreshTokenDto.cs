using System.Text.Json.Serialization;

namespace VkNet.AudioBypassService.Models.Auth;

public record AuthRefreshWebviewRefreshTokenDto(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);