using System.Collections.ObjectModel;
using VkNet.AudioBypassService.Models.Auth;

namespace VkNet.AudioBypassService.Models.Ecosystem;

public record EcosystemGetVerificationMethodsResponse(ReadOnlyCollection<EcosystemVerificationMethod> Methods);

public record EcosystemVerificationMethod(LoginWay Name, int Priority, int Timeout, string Info, bool CanFallback);