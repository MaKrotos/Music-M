using System.Text.Json.Serialization;

namespace VkNet.AudioBypassService.Models.Auth;

public record AuthRefreshSilentTokenDto(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("uuid")] string Uuid
);