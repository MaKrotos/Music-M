using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MusicX.Core.Services;
using NLog;
using VK_UI3.Converters;
using VkNet;
using VkNet.Abstractions;
using VkNet.Exception;

using VkNet.Model;
using VkNet.Utils;

namespace MusicX.Services;

public class VkBridgeService : IDisposable
{
    private readonly IVkApiInvoke _vkApi;
    private readonly Logger _log;
    private readonly VkService _vkService;

    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new VkBoolJsonConverter() }
    };

    private string? _accessToken;
    private string _currentAppId = "app0";
    private string _currentUrl = "https://vk.ru/";
    private string _storagePath = "";
    private VkApi? _vkApiInstance;
    private long? _currentUserId;
    private string? _vkAuthToken;

    public VkBridgeService(IVkApiInvoke vkApi, Logger log, VkService vkService)
    {
        _vkApi = vkApi;
        _log = log;
        _vkService = vkService;
    }

    public void Load(string appId, string url, VkApi? vkApi = null)
    {
        _currentAppId = appId;
        _currentUrl = url;
        _storagePath = Path.Join(StaticService.UserDataFolder.FullName, $"{appId}.json");
        _vkApiInstance = vkApi;

        // Получаем ID текущего пользователя и токен
        if (_vkApiInstance != null && _vkApiInstance.UserId.HasValue)
        {
            _currentUserId = _vkApiInstance.UserId.Value;
            _vkAuthToken = _vkApiInstance.Token;

            _log.Info($"Bridge loaded for user {_currentUserId}, app: {appId}, url: {url}");
        }
        else
        {
            _log.Warn("Bridge loaded without user authentication");
        }
    }

    public async Task<JsonObject> HandleBridgeCallAsync(string method, JsonObject parameters, string? requestId = null)
    {
        try
        {
            var result = await ExecuteMethodAsync(method, parameters);
            return CreateSuccessResponse(method, result, requestId);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(method, ex, requestId);
        }
    }

    private async Task<object?> ExecuteMethodAsync(string method, JsonObject parameters)
    {
        return method switch
        {
            "VKWebAppInit" => await VKWebAppInitAsync(parameters),
            "VKWebAppGetConfig" => await VKWebAppGetConfigAsync(parameters),
            "VKWebAppCallAPIMethod" => await VKWebAppCallAPIMethodAsync(parameters),
            "VKWebAppAddToCommunity" => await VKWebAppAddToCommunityAsync(parameters),
            "VKWebAppGetAuthToken" => await VKWebAppGetAuthTokenAsync(parameters),
            "VKWebAppGetLaunchParams" => await VKWebAppGetLaunchParamsAsync(parameters),
            "VKWebAppStorageGet" => await VKWebAppStorageGetAsync(parameters),
            "VKWebAppStorageGetKeys" => await VKWebAppStorageGetKeysAsync(parameters),
            "VKWebAppStorageSet" => await VKWebAppStorageSetAsync(parameters),
            "VKWebAppGetUserInfo" => await VKWebAppGetUserInfoAsync(parameters),
            "VKWebAppGetEmail" => await VKWebAppGetEmailAsync(parameters),
            "VKWebAppGetPhoneNumber" => await VKWebAppGetPhoneNumberAsync(parameters),
            "VKWebAppGetFriends" => await VKWebAppGetFriendsAsync(parameters),
            "VKWebAppGetClientVersion" => await VKWebAppGetClientVersionAsync(parameters),
            "SetSupportedHandlers" => await SetSupportedHandlersAsync(parameters),
            _ => throw new NotSupportedException($"Method {method} is not supported")
        };
    }

    private async Task<object> VKWebAppInitAsync(JsonObject parameters)
    {
        return new { Result = true };
    }

    private async Task<object> VKWebAppGetConfigAsync(JsonObject parameters)
    {
        return new
        {
            App = "vkclient",
            AppId = _currentAppId,
            Appearance = "dark",
            Scheme = "space_gray",
            VkPayAppId = "7675483",
            StartupUrl = _currentUrl
        };
    }

    private async Task<object> VKWebAppCallAPIMethodAsync(JsonObject parameters)
    {
        var methodName = parameters["method"]?.ToString();
        var jsonParams = parameters["params"]?.ToString();

        if (string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(jsonParams))
            throw new ArgumentException("Invalid parameters");

        var parsedParams = JsonNode.Parse(jsonParams)!.AsObject();
        var vkParameters = new VkParameters(parsedParams.ToImmutableDictionary(
            b => b.Key,
            b => b.Value?.ToString()));

        // Добавляем токен если он есть
        if (!vkParameters.ContainsKey("access_token") && _vkAuthToken != null)
        {
            vkParameters.Add("access_token", _vkAuthToken);
        }

        vkParameters.Add("v", "5.199");
        vkParameters.Add("https", "1");

        try
        {
            var response = await _vkApi.CallAsync(methodName, vkParameters);
            var jsonNode = JsonNode.Parse(response.RawJson);

            return new
            {
                Response = jsonNode,
                RequestId = parameters["request_id"]?.ToString()
            };
        }
        catch (VkApiException vkEx)
        {
            throw new Exception($"VK API Error: {vkEx.Message}", vkEx);
        }
    }

    private async Task<object> VKWebAppAddToCommunityAsync(JsonObject parameters)
    {
        return new
        {
            ErrorType = "client_error",
            ErrorData = new
            {
                ErrorCode = 4,
                ErrorReason = "Unsupported operation",
                ErrorDescription = "This operation is not supported in this client"
            }
        };
    }

    private async Task<object> VKWebAppGetAuthTokenAsync(JsonObject parameters)
    {
        var appId = parameters["app_id"]?.GetValue<long>() ?? 0;
        var scope = parameters["scope"]?.ToString() ?? "";

        try
        {
            var response = await _vkService.GetMiniAppCredentialToken(_currentUrl, scope);
            _accessToken = response.AccessToken;

            // Используем доступные свойства
            return new
            {
                response.AccessToken,
                Scope = scope
                // ExpiresIn может не быть в CredentialResponse
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get auth token: {ex.Message}", ex);
        }
    }

    private async Task<object> VKWebAppGetLaunchParamsAsync(JsonObject parameters)
    {
        try
        {
            var appId = long.Parse(_currentAppId["app".Length..]);
            var response = await _vkService.GetMiniAppLaunchParams(appId);

            // Создаем JsonObject из ответа
            var launchParams = new JsonObject();

            // Сериализуем response в JsonObject
            if (response != null)
            {
                var jsonNode = JsonNode.Parse(JsonSerializer.Serialize(response));
                if (jsonNode is JsonObject responseObj)
                {
                    foreach (var prop in responseObj)
                    {
                        // Копируем значение, создавая копию через Parse
                        launchParams[prop.Key] = JsonNode.Parse(prop.Value?.ToJsonString() ?? "null");
                    }
                }
            }

            // Добавляем ID пользователя если он авторизован
            if (_currentUserId.HasValue)
            {
                launchParams["vk_user_id"] = _currentUserId.Value;
                launchParams["sign"] = "TODO_SIGN"; // Нужно генерировать подпись
            }

            if (_vkAuthToken != null)
            {
                launchParams["access_token"] = _vkAuthToken;
                launchParams["vk_access_token_settings"] = "all";
            }

            // Добавляем обязательные параметры если их нет
            if (!launchParams.ContainsKey("vk_app_id"))
                launchParams["vk_app_id"] = _currentAppId["app".Length..];

            if (!launchParams.ContainsKey("vk_is_app_user"))
                launchParams["vk_is_app_user"] = 1;

            if (!launchParams.ContainsKey("vk_language"))
                launchParams["vk_language"] = "ru";

            if (!launchParams.ContainsKey("vk_ref"))
                launchParams["vk_ref"] = "other";

            if (!launchParams.ContainsKey("vk_platform"))
                launchParams["vk_platform"] = "desktop_web";

            return launchParams;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get launch params");

            // Возвращаем базовые параметры
            var launchParams = new JsonObject
            {
                ["vk_user_id"] = _currentUserId ?? 0,
                ["vk_app_id"] = _currentAppId["app".Length..],
                ["vk_is_app_user"] = 1,
                ["vk_are_notifications_enabled"] = 0,
                ["vk_language"] = "ru",
                ["vk_ref"] = "other",
                ["vk_access_token_settings"] = "all",
                ["vk_platform"] = "desktop_web",
                ["sign"] = "dummy_sign_for_dev"
            };

            if (_vkAuthToken != null)
            {
                launchParams["access_token"] = _vkAuthToken;
            }

            return launchParams;
        }
    }
    private async Task<object> VKWebAppStorageGetAsync(JsonObject parameters)
    {
        var keys = parameters["keys"]?.AsArray()
            .Select(k => k?.ToString())
            .Where(k => !string.IsNullOrEmpty(k))
            .ToArray() ?? Array.Empty<string>();

        var storage = await ReadStorageAsync();

        return new
        {
            Keys = storage
                .Where(k => keys.Contains(k.Key))
                .Select(b => new { b.Key, b.Value })
                .ToArray()
        };
    }

    private async Task<object> VKWebAppStorageGetKeysAsync(JsonObject parameters)
    {
        var count = parameters["count"]?.GetValue<int>() ?? 100;
        var offset = parameters["offset"]?.GetValue<int>() ?? 0;

        var storage = await ReadStorageAsync();

        return new
        {
            Keys = storage
                .Skip(offset)
                .Take(count)
                .Select(b => b.Key)
                .ToArray()
        };
    }

    private async Task<object> VKWebAppStorageSetAsync(JsonObject parameters)
    {
        var key = parameters["key"]?.ToString();
        var value = parameters["value"]?.ToString();

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key is required");

        await WriteStorageAsync(key!, value ?? "");
        return new { Result = true };
    }

    private async Task<object> VKWebAppGetUserInfoAsync(JsonObject parameters)
    {
        if (!_currentUserId.HasValue || _vkApiInstance == null)
        {
            return new
            {
                ErrorType = "client_error",
                ErrorData = new
                {
                    ErrorCode = 5,
                    ErrorReason = "User not authorized",
                    ErrorDescription = "Please log in first"
                }
            };
        }

        try
        {
            var userId = parameters["user_id"]?.GetValue<long>() ?? _currentUserId.Value;
            var users = await _vkApiInstance.Users.GetAsync(new[] { userId });
            var user = users.FirstOrDefault();

            if (user == null)
                throw new Exception("User not found");

            // Преобразуем Sex в строку - исправленная часть
            string sexStr = "unknown";
            if (user.Sex != null)
            {
                sexStr = user.Sex.ToString(); // Просто вызываем ToString()
            }

            return new
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Sex = sexStr,
                Photo = user.Photo50?.ToString(),
                PhotoMedium = user.Photo100?.ToString(),
                PhotoBig = user.Photo200?.ToString()
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get user info: {ex.Message}", ex);
        }
    }

    private async Task<object> VKWebAppGetEmailAsync(JsonObject parameters)
    {
        return new
        {
            ErrorType = "client_error",
            ErrorData = new
            {
                ErrorCode = 6,
                ErrorReason = "Not implemented",
                ErrorDescription = "Email access is not implemented"
            }
        };
    }

    private async Task<object> VKWebAppGetPhoneNumberAsync(JsonObject parameters)
    {
        return new
        {
            ErrorType = "client_error",
            ErrorData = new
            {
                ErrorCode = 6,
                ErrorReason = "Not implemented",
                ErrorDescription = "Phone number access is not implemented"
            }
        };
    }

    private async Task<object> VKWebAppGetFriendsAsync(JsonObject parameters)
    {
        if (_vkApiInstance == null)
        {
            return new
            {
                ErrorType = "client_error",
                ErrorData = new
                {
                    ErrorCode = 5,
                    ErrorReason = "User not authorized",
                    ErrorDescription = "Please log in first"
                }
            };
        }

        try
        {
            var multi = parameters["multi"]?.GetValue<bool>() ?? false;

            var friends = await _vkApiInstance.Friends.GetAsync(new VkNet.Model.RequestParams.FriendsGetParams
            {
                UserId = _currentUserId,
                Fields = VkNet.Enums.Filters.ProfileFields.FirstName | VkNet.Enums.Filters.ProfileFields.LastName |
                         VkNet.Enums.Filters.ProfileFields.Photo50
            });

            return new
            {
                Count = friends.Count,
                Items = friends.Select(f => new
                {
                    f.Id,
                    f.FirstName,
                    f.LastName,
                    Photo = f.Photo50?.ToString()
                }).ToArray()
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get friends: {ex.Message}", ex);
        }
    }

    private async Task<object> VKWebAppGetClientVersionAsync(JsonObject parameters)
    {
        return new
        {
            Version = "8.154",
            Platform = "android"
        };
    }

    private async Task<object> SetSupportedHandlersAsync(JsonObject parameters)
    {
        var methods = new[]
        {
            "VKWebAppInit",
            "VKWebAppGetConfig",
            "VKWebAppCallAPIMethod",
            "VKWebAppAddToCommunity",
            "VKWebAppGetAuthToken",
            "VKWebAppGetLaunchParams",
            "VKWebAppStorageGet",
            "VKWebAppStorageGetKeys",
            "VKWebAppStorageSet",
            "VKWebAppGetUserInfo",
            "VKWebAppGetEmail",
            "VKWebAppGetPhoneNumber",
            "VKWebAppGetFriends",
            "VKWebAppGetClientVersion",
            "SetSupportedHandlers"
        };

        return new { SupportedHandlers = methods };
    }

    private JsonObject CreateSuccessResponse(string method, object data, string? requestId = null)
    {
        var json = JsonSerializer.SerializeToNode(new
        {
            type = method,
            data
        }, _options)!.AsObject();

        if (!string.IsNullOrEmpty(requestId))
        {
            json["request_id"] = requestId;
        }

        return json;
    }

    private JsonObject CreateErrorResponse(string method, Exception e, string? requestId = null)
    {
        JsonObject errorData;

        if (e is VkApiException apiException)
        {
            errorData = JsonSerializer.SerializeToNode(new
            {
                error_type = "api_error",
                error_data = new
                {
                    apiException.ErrorCode,
                    error_msg = apiException.Message,
                    request_params = apiException.RequestParams?.Keys
                }
            }, _options)!.AsObject();
        }
        else
        {
            errorData = JsonSerializer.SerializeToNode(new
            {
                error_type = "client_error",
                error_data = new
                {
                    error_code = 1,
                    error_reason = e.Message,
                    error_description = $"{e.GetType().Name}: {e.Message}"
                }
            }, _options)!.AsObject();
        }

        _log.Error(e, "Failed to call {Method} from bridge call", method);

        var response = JsonSerializer.SerializeToNode(new
        {
            type = method,
            data = errorData
        }, _options)!.AsObject();

        if (!string.IsNullOrEmpty(requestId))
        {
            response["request_id"] = requestId;
        }

        return response;
    }

    private async Task<IReadOnlyDictionary<string, string>> ReadStorageAsync()
    {
        if (!File.Exists(_storagePath))
            return ImmutableDictionary<string, string>.Empty;

        try
        {
            await using var stream = File.OpenRead(_storagePath);
            var result = await JsonSerializer.DeserializeAsync<IReadOnlyDictionary<string, string>>(stream);
            return result ?? ImmutableDictionary<string, string>.Empty;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to read storage");
            return ImmutableDictionary<string, string>.Empty;
        }
    }

    private async Task WriteStorageAsync(string key, string value)
    {
        try
        {
            var storage = await ReadStorageAsync();
            var modified = storage.Append(new(key, value)).ToImmutableDictionary();

            await using var stream = File.Create(_storagePath);
            await JsonSerializer.SerializeAsync(stream, modified);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to write storage");
            throw;
        }
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }
}