// ReSharper disable once CheckNamespace

using System.Text.Json.Serialization;

namespace Windows.Win32;

internal static partial class PInvoke
{
    public const uint WebauthnAuthenticatorAttachmentAny = 0;
    public const uint WebauthnAuthenticatorAttachmentPlatform = 1;
    public const uint WebauthnAuthenticatorAttachmentCrossPlatform = 2;
    public const uint WebauthnAuthenticatorAttachmentCrossPlatformU2FV2 = 3;
    public const string WebauthnCredentialTypePublicKey = "public-key";
    
    public record SecurityKeyClientData(
        [property: JsonPropertyName("type"), JsonPropertyOrder(1)]
        string DataType,
        [property: JsonPropertyName("challenge"), JsonPropertyOrder(2)]
        string Challenge,
        [property: JsonPropertyName("origin"), JsonPropertyOrder(3)]
        string Origin,
        [property: JsonPropertyName("androidPackageName"), JsonPropertyOrder(4)]
        string AndroidPackageName = "com.vkontakte.android")
    {
        public const string MakeCredential = "webauthn.create";
        public const string GetAssertion = "webauthn.get";

        public const string U2FRegister = "navigator.id.finishEnrollment";
        public const string U2FSign = "navigator.id.getAssertion";
    }
}
