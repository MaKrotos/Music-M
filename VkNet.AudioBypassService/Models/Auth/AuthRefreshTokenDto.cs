using System.Text.Json.Serialization;

namespace VkNet.AudioBypassService.Models.Auth;

public record AuthRefreshTokenDto(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("user_id")] long UserId,
    [property: JsonPropertyName("banned")] bool Banned,
    [property: JsonPropertyName("access_token")] AuthRefreshAccessTokenDto AccessToken,
    [property: JsonPropertyName("webview_access_token")] AuthRefreshWebviewAccessTokenDto WebviewAccessToken,
    [property: JsonPropertyName("webview_refresh_token")] AuthRefreshWebviewRefreshTokenDto WebviewRefreshToken,
    [property: JsonPropertyName("silent_token")] AuthRefreshSilentTokenDto SilentToken
);