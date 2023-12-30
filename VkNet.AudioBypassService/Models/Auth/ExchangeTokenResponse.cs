using VkNet.Model;

namespace VkNet.AudioBypassService.Models.Auth;

public record ExchangeTokenResponse(string ExchangeToken, User Profile);