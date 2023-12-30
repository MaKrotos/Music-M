namespace VkNet.AudioBypassService.Models.Auth;

public record AnonymousTokenResponse(string Token, int ExpiredAt);