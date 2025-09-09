using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Abstractions.Categories;
using VkNet.Extensions.DependencyInjection;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Categories;

public class LoginCategory : ILoginCategory
{
    private readonly IVkTokenStore _tokenStore;
    private readonly IRestClient _restClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public LoginCategory(IVkTokenStore tokenStore, IRestClient restClient)
    {
        _tokenStore = tokenStore;
        _restClient = restClient;
    }

    public async Task ConnectAsync(string uuid)
    {
        var parameters = new VkParameters
        {
            { "uuid", uuid },
            { "version", 1 },
            { "app_id", 7913379 }
        };
        
        var response = await _restClient.PostAsync(new("https://login.vk.ru/?act=connect_internal"), parameters, Encoding.UTF8);

        if (!response.IsSuccess)
            throw new HttpRequestException("Error connecting as desktop", null, response.StatusCode);

        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(response.Value, _jsonSerializerOptions);

        if (loginResponse.Type != "okay")
            throw new AuthenticationException();
    }

    public async Task ConnectAuthCodeAsync(string token, string uuid)
    {
        var parameters = new VkParameters
        {
            { "uuid", uuid },
            { "version", 1 },
            { "app_id", 7913379 },
            { "token", token }
        };
        
        var response = await _restClient.PostAsync(new("https://login.vk.ru/?act=connect_code_auth"), parameters, Encoding.UTF8);

        if (!response.IsSuccess)
            throw new HttpRequestException("Error connecting auth code", null, response.StatusCode);

        var (type, loginResponse) = JsonSerializer.Deserialize<LoginResponse<LoginResponseAccessToken>>(response.Value, _jsonSerializerOptions);

        if (type != "okay" || loginResponse.ResponseType != "auth_token")
            throw new AuthenticationException();

        await _tokenStore.SetAsync(loginResponse.AccessToken);
    }
}

public record LoginResponse(string Type);
public record LoginResponse<TData>(string Type, TData Data) : LoginResponse(Type) where TData : class;

public record LoginResponseAccessToken(string ResponseType, string AccessToken);