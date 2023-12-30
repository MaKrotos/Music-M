using System.Threading.Tasks;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Models.Ecosystem;

namespace VkNet.AudioBypassService.Abstractions.Categories;

public interface IEcosystemCategory
{
    Task<EcosystemSendOtpResponse> SendOtpSmsAsync(string sid);
    Task<EcosystemSendOtpResponse> SendOtpPushAsync(string sid);
    Task<EcosystemSendOtpResponse> SendOtpCallResetAsync(string sid);
    Task<EcosystemCheckOtpResponse> CheckOtpAsync(string sid, LoginWay verificationMethod, string code);
    Task<EcosystemGetVerificationMethodsResponse> GetVerificationMethodsAsync(string sid);
}