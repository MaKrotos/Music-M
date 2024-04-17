using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VkNet.AudioBypassService.Models.Auth;

public record PasskeyBeginResponse(string Sid, string PasskeyData);
public record PasskeyDataResponse(string Challenge, int Timeout, string RpId, IReadOnlyCollection<PasskeyAllowCredentials> AllowCredentials, string UserVerification);
public record PasskeyAllowCredentials(string Type, string Id);