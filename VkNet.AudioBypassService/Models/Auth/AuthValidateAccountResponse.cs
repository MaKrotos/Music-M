using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using VkNet.Enums.SafetyEnums;
using VkNet.Utils.JsonConverter;

namespace VkNet.AudioBypassService.Models.Auth;

public record AuthValidateAccountResponse(bool IsEmail, bool IsPhone, AuthFlow FlowName, ReadOnlyCollection<AuthType> FlowNames, string Sid, NextVerificationStep NextStep);

public record NextVerificationStep(LoginWay VerificationMethod, bool HasAnotherVerificationMethods, [CanBeNull] string ExternalId);

[JsonConverter(typeof(SafetyEnumJsonConverter))]
public class AuthFlow : SafetyEnum<AuthFlow>
{
    public static readonly AuthFlow NeedPasskey = RegisterPossibleValue("need_passkey_otp");
    public static readonly AuthFlow NeedPassword = RegisterPossibleValue("need_password_and_validation");
    public static readonly AuthFlow NeedValidation = RegisterPossibleValue("need_validation");
}

[JsonConverter(typeof(SafetyEnumJsonConverter))]
public class AuthType : SafetyEnum<AuthType>
{
    public static readonly AuthType Password = RegisterPossibleValue("password");
    public static readonly AuthType Passkey = RegisterPossibleValue("passkey");
    public static readonly AuthType Otp = RegisterPossibleValue("otp");
}

[JsonConverter(typeof(SafetyEnumJsonConverter))]
public class LoginWay : SafetyEnum<LoginWay>
{
    public static readonly LoginWay Email = RegisterPossibleValue("email");
    public static readonly LoginWay Push = RegisterPossibleValue("push");
    public static readonly LoginWay Sms = RegisterPossibleValue("sms");
    public static readonly LoginWay CallReset = RegisterPossibleValue("callreset");
    public static readonly LoginWay Password = RegisterPossibleValue("password");
    public static readonly LoginWay ReserveCode = RegisterPossibleValue("reserve_code");
    public static readonly LoginWay Codegen = RegisterPossibleValue("codegen");
    public static readonly LoginWay LibVerify = RegisterPossibleValue("libverify");
    public static readonly LoginWay Passkey = RegisterPossibleValue("passkey");
    public static readonly LoginWay TwoFactorLibVerify = RegisterPossibleValue("2fa_libverify");
    public static readonly LoginWay None = RegisterPossibleValue(string.Empty);
    public static readonly LoginWay TwoFactorApp = RegisterPossibleValue("2fa_app");
    public static readonly LoginWay TwoFactorCallReset = RegisterPossibleValue("2fa_callreset");
    public static readonly LoginWay TwoFactorSms = RegisterPossibleValue("2fa_sms");
    public static readonly LoginWay TwoFactorPush = RegisterPossibleValue("2fa_push");
    public static readonly LoginWay TwoFactorEmail = RegisterPossibleValue("2fa_email");
}

public record NeedValidationAuthResponse(LoginWay ValidationType, string ValidationSid, [CanBeNull] string PhoneMask, LoginWay ValidationResend, [CanBeNull] string ValidationExternalId);