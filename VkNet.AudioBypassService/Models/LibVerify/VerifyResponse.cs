using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VkNet.AudioBypassService.Models.LibVerify;

public record VerifyResponse(string SessionId, string VerificationUrl, VerifyResponseStatus Status, IReadOnlyList<VerifyChecks> Checks, int CodeLength, CodeType CodeType);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VerifyResponseStatus
{
    None,
    Ok
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VerifyChecks
{
    None,
    Sms
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CodeType
{
    None,
    Numeric
}