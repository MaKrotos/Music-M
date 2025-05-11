using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MusicX.Core.Services;
using NLog;
using VK_UI3.Converters;
using VkNet.Abstractions;
using VkNet.Exception;
using VkNet.Utils;

namespace MusicX.Services;

[ClassInterface(ClassInterfaceType.AutoDual)]
[ComVisible(true)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class VkBridgeService(IVkApiInvoke vkApi, Logger log, VkService vkService)
{
    #region Events

    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters =
        {
            new VkBoolJsonConverter()
        }
    };

    private string? _nextRequestId;
    private string? _accessToken;

    public event Action<string>? VKWebAppEvent;

    private void DispatchEvent<T>(string type, T data)
    {
        var json = JsonSerializer.SerializeToNode(new
        {
            type,
            data
        }, _options)!;
        
        if (_nextRequestId is not null)
        {
            json["data"]!.AsObject()["request_id"] = _nextRequestId;
            _nextRequestId = null;
        }
        
        VKWebAppEvent?.Invoke(json.ToJsonString());
    }

    #endregion

    private string _currentAppId = "app0";
    private string _currentUrl = "https://vk.com/";
    private string _storagePath = "";

    internal void Load(string appId, string url)
    {
        _currentAppId = appId;
        _currentUrl = url;
        _storagePath = Path.Join(StaticService.UserDataFolder.FullName, $"{appId}.json");
    }
    
    public void SetNextRequestId(string requestId)
    {
        _nextRequestId = requestId;
    }
    
    public void VKWebAppInit()
    {
        DispatchEvent(nameof(VKWebAppInit), new
        {
            Result = true
        });
    }

    public void VKWebAppGetConfig()
    {
        DispatchEvent(nameof(VKWebAppGetConfig), new
        {
            App = "vkclient",
            AppId = _currentAppId,
            Appearance =   "dark" ,
            Scheme =  "space_gray"
        });
    }

    public async void VKWebAppCallAPIMethod(string method, string jsonParams)
    {
        try
        {
            var parameters = JsonNode.Parse(jsonParams)!.AsObject();
            var vkParameters = new VkParameters(parameters.ToImmutableDictionary(b => b.Key, b => b.Value?.ToString()));
        
            if (_accessToken is not null)
                vkParameters.TryAdd("access_token", _accessToken);

            vkParameters.TryAdd("https", "1");
            
            var response = await vkApi.CallAsync(method, vkParameters);

            DispatchEvent(nameof(VKWebAppCallAPIMethod), new
            {
                Response = JsonNode.Parse(response.RawJson)
            });
        }
        catch (Exception e)
        {
            DispatchException(nameof(VKWebAppCallAPIMethod), e);
        }
    }

    public void VKWebAppAddToCommunity()
    {
        DispatchEvent(nameof(VKWebAppAddToCommunity), new
        {
            ErrorType = "client_error",
            ErrorData = new
            {
                ErrorCode = 4,
                ErrorReason = "Unsupported operation",
                ErrorDescription = "This operation is not supported"
            }
        });
    }

    public async void VKWebAppGetAuthToken(long appId, string scope)
    {
        try
        {
            var response = await vkService.GetMiniAppCredentialToken(_currentUrl, scope);

            _accessToken = response.AccessToken;
            
            DispatchEvent(nameof(VKWebAppGetAuthToken), new
            {
                response.AccessToken,
                Scope = scope
            });
        }
        catch (Exception e)
        {
            DispatchException(nameof(VKWebAppGetAuthToken), e);
        }
    }

    public async void VKWebAppGetLaunchParams()
    {
        try
        {
            var appId = long.Parse(_currentAppId["app".Length..]);

            var response = await vkService.GetMiniAppLaunchParams(appId);
            
            DispatchEvent(nameof(VKWebAppGetLaunchParams), response);
        }
        catch (Exception e)
        {
            DispatchException(nameof(VKWebAppGetLaunchParams), e);
        }
    }

    public async void VKWebAppStorageGet(object[] keys)
    {
        try
        {
            var storage = await ReadStorageAsync();
            
            DispatchEvent(nameof(VKWebAppStorageGet), new
            {
                Keys = storage.IntersectBy(keys.OfType<string>(), b => b.Key)
                    .Select(b => new { b.Key, b.Value })
                    .ToArray()
            });
        }
        catch (Exception e)
        {
            DispatchException(nameof(VKWebAppStorageGet), e);
        }
    }

    public async void VKWebAppStorageGetKeys(int count, int offset)
    {
        try
        {
            var storage = await ReadStorageAsync();
            
            DispatchEvent(nameof(VKWebAppStorageGetKeys), new
            {
                Keys = storage
                    .Skip(offset)
                    .Take(count)
                    .Select(b => b.Key)
                    .ToArray()
            });
        }
        catch (Exception e)
        {
            DispatchException(nameof(VKWebAppStorageGetKeys), e);
        }
    }
    
    public async void VKWebAppStorageSet(string key, string value)
    {
        try
        {
            await WriteStorageAsync(key, value);
            
            DispatchEvent(nameof(VKWebAppStorageSet), new
            {
                Result = true
            });
        }
        catch (Exception e)
        {
            DispatchException(nameof(VKWebAppStorageSet), e);
        }
    }

    public void SetSupportedHandlers()
    {
        DispatchEvent(nameof(SetSupportedHandlers), new SetSupportedHandlersResponse(typeof(VkBridgeService)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(b => b.Name)
            .Where(b => b.StartsWith("VK"))
            .ToArray()));
    }
    
    // vk moment
    private record SetSupportedHandlersResponse([property: JsonPropertyName("supportedHandlers")] string[] SupportedHandlers);

    private void DispatchException(string methodName, Exception e)
    {
        if (e is VkApiException apiException)
        {
            DispatchEvent(methodName, new
            {
                ErrorType = "api_error",
                ErrorData = new
                {
                    apiException.ErrorCode,
                    ErrorMsg = apiException.Message,
                    RequestParams = apiException.RequestParams?.Keys
                }
            });
            
            return;
        }
        
        DispatchEvent(methodName, new
        {
            ErrorType = "client_error",
            ErrorData = new
            {
                ErrorCode = 1,
                ErrorReason = e.Message,
                ErrorDescription = $"{e.GetType()}: {e.Message}"
            }
        });
            
        log.Error(e, "Failed to call {Method} from bridge call", methodName);
    }

    private async Task<IReadOnlyDictionary<string, string>> ReadStorageAsync()
    {
        if (!File.Exists(_storagePath))
            return ImmutableDictionary<string, string>.Empty;

        await using var stream = File.OpenRead(_storagePath);
        
        var result = await JsonSerializer.DeserializeAsync<IReadOnlyDictionary<string, string>>(stream);
        
        return result ?? ImmutableDictionary<string, string>.Empty;
    }

    private async Task WriteStorageAsync(string key, string value)
    {
        var storage = await ReadStorageAsync();

        var modified = storage.Append(new(key, value)).ToImmutableDictionary();
        
        await using var stream = File.Create(_storagePath);
        
        await JsonSerializer.SerializeAsync(stream, modified);
    }


}