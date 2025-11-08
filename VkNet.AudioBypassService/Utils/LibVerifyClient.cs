using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VkNet.AudioBypassService.Models.LibVerify;

namespace VkNet.AudioBypassService.Utils;

public class LibVerifyClient
{
    private static readonly Guid DeviceId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    private const string DeviceName = "Google Pixel 6";
    
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new("https://clientapi.mail.ru/fcgi-bin/")
    };

    public LibVerifyClient()
    {
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "Dalvik/2.1.0 (Linux; U; Android 12; Pixel 6 Build/SP2A.220505.006)");
    }
    
    public Task<VerifyResponse> VerifyAsync(string externalId, string phone)
    {
        return _httpClient.GetFromJsonAsync<VerifyResponse>(
            $"verify?application=VK&application_id=5b55d763-87a7-496b-828a-fa886a261827&auth_type=VKCONNECT&capabilities=call_number_fragment,call_session_hash,background_verify,ping_v2,request_id,safety_net_v3,mow,route_info,mobileid_redirects,sms_retriever&checks=sms,push,callui&device_id={DeviceId}&device_name={DeviceName}&env=gps&language=ru_RU&libverify_build=251&libverify_version=2.7.1&os_version=12&phone={phone}&platform=android&request_id={RandomString.Generate(32)}&service=vk_otp_auth&session_id={RandomString.Generate(32)}&system_id=1b9061826218bd36&version=17498&external_id={externalId}&signature=3dabe0188c3042f8076c293537f3f11c");
    }

    public Task<AttemptResponse> AttemptAsync(string url, string code)
    {
        return _httpClient.GetFromJsonAsync<AttemptResponse>(
            $"{url}&application=VK&application_id=5b55d763-87a7-496b-828a-fa886a261827&code={code}&code_source=USER_INPUT&signature=deb56ad0762797cdccdb300647b85f27"
        );
    }
}