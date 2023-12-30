using System.Collections.ObjectModel;

namespace VkNet.AudioBypassService.Models.Auth;

public record AuthRefreshTokensResponse(ReadOnlyCollection<RefreshedSlot> Success);

public record RefreshedSlot(int Index, long UserId, TokenInfo AccessToken);

public record TokenInfo(string Token, long ExpiresIn);
