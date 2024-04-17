using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Abstractions.Categories;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Models.Ecosystem;

namespace VkNet.AudioBypassService.Categories;

public class EcosystemCategory : IEcosystemCategory
{
    private readonly IVkApiInvoke _invoke;

    public EcosystemCategory(IVkApiInvoke invoke)
    {
        _invoke = invoke;
    }

    public Task<EcosystemSendOtpResponse> SendOtpSmsAsync(string sid)
    {
        return _invoke.CallAsync<EcosystemSendOtpResponse>("ecosystem.sendOtpSms", new()
        {
            { "sid", sid },
            { "api_id", 2274003 }
        });
    }

    public Task<EcosystemSendOtpResponse> SendOtpPushAsync(string sid)
    {
        return _invoke.CallAsync<EcosystemSendOtpResponse>("ecosystem.sendOtpPush", new()
        {
            { "sid", sid },
            { "api_id", 2274003 }
        });
    }

    public Task<EcosystemSendOtpResponse> SendOtpCallResetAsync(string sid)
    {
        return _invoke.CallAsync<EcosystemSendOtpResponse>("ecosystem.sendOtpCallReset", new()
        {
            { "sid", sid },
            { "api_id", 2274003 }
        });
    }

    public Task<EcosystemCheckOtpResponse> CheckOtpAsync(string sid, LoginWay verificationMethod, string code)
    {
        return _invoke.CallAsync<EcosystemCheckOtpResponse>("ecosystem.checkOtp", new()
        {
            { "sid", sid },
            { "api_id", 2274003 },
            { "verification_method", verificationMethod },
            { "code", code }
        });
    }

    public Task<EcosystemGetVerificationMethodsResponse> GetVerificationMethodsAsync(string sid)
    {
        return _invoke.CallAsync<EcosystemGetVerificationMethodsResponse>("ecosystem.getVerificationMethods", new()
        {
            { "sid", sid },
            { "api_id", 2274003 }
        });
    }
}