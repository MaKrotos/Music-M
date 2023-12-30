using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VkNet.AudioBypassService.Models.LibVerify;

namespace VkNet.AudioBypassService.Utils;

public class LibVerifyClient
{
    private static readonly Guid DeviceId = new("4bdec837-2820-4854-8c73-61a79577482e");
    private const string DeviceName = "MusicX+MusicX";
    
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new("https://clientapi.mail.ru/fcgi-bin/")
    };

    public LibVerifyClient()
    {
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "Dalvik/2.1.0 (Linux; U; Android 12; MusicX Build/QQ3A.200705.002)");
    }
    
    public Task<VerifyResponse> VerifyAsync(string externalId, string phone)
    {
        return _httpClient.GetFromJsonAsync<VerifyResponse>(
            $"verify?application=VK&application_id=5b55d763-87a7-496b-828a-fa886a261827&auth_type=VKCONNECT&capabilities=call_number_fragment,call_session_hash,background_verify,ping_v2,request_id,safety_net_v3,mow,route_info,mobileid_redirects,sms_retriever&checks=sms,push,callui&device_id={DeviceId}&device_name={DeviceName}&env=gps&language=ru_RU&libverify_build=251&libverify_version=2.7.1&os_version=10&phone={phone}&platform=android&request_id={RandomString.Generate(32)}&service=vk_otp_auth&session_id={RandomString.Generate(32)}&system_id=1b9061826218bd36&version=17498&external_id={externalId}&signature=3dabe0188c3042f8076c293537f3f11c");
    }

    public Task<AttemptResponse> AttemptAsync(string url, string code)
    {
        return _httpClient.GetFromJsonAsync<AttemptResponse>(
            $"{url}&application=VK&application_id=5b55d763-87a7-496b-828a-fa886a261827&code={code}&code_source=USER_INPUT&signature=deb56ad0762797cdccdb300647b85f27"
        );
    }
}