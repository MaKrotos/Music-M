using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VkNet.AudioBypassService.Models.Auth;

public record AuthRefreshTokensResponseDto(
    [property: JsonPropertyName("success")] IReadOnlyList<AuthRefreshTokenDto> Success,
    [property: JsonPropertyName("errors")] IReadOnlyList<object> Errors
);