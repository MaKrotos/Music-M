using JetBrains.Annotations;

namespace VkNet.AudioBypassService.Models.Auth;

public record AuthCheckResponse(AuthCheckStatus Status, int ExpiresIn, [CanBeNull] string SuperAppToken, [CanBeNull] string AccessToken, bool NeedPassword, bool IsPartial, int ProviderAppId);

public enum AuthCheckStatus
{
    Continue,
    ConfirmOnPhone,
    Ok,
    Expired = 4,
    Loading = 200
}