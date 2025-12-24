using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using MusicX.Services;
using MvvmHelpers;
using NLog.Fluent;
using Octokit;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.VKs;
using VkNet;
using VkNet.Abstractions;
using VkNet.Model;

namespace VK_UI3.Views
{
    public class MiniAppViewModel(string appId, string url, VkApi? vkApi = null) : ObservableObject
    {
        public string AppId { get; } = appId;
        public string Url { get; } = url;
        public VkApi? VkApi { get; } = vkApi;

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private string _errorDetails = string.Empty;
        public string ErrorDetails
        {
            get => _errorDetails;
            set => SetProperty(ref _errorDetails, value);
        }
    }

    public sealed partial class MiniAppView : Microsoft.UI.Xaml.Controls.Page
    {
        private MiniAppViewModel? _viewModel;
        private VkBridgeService? _bridgeService;
        private bool _isNavigating;

        public MiniAppView()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not MiniAppViewModel viewModel)
            {
                // Если передали просто строки
                if (e.Parameter is object[] parameters && parameters.Length >= 2)
                {
                    var appId = parameters[0] as string ?? "app0";
                    var url = parameters[1] as string ?? "https://vk.com";
                    var vkApi = parameters.Length > 2 ? parameters[2] as VkApi : null;

                    viewModel = new MiniAppViewModel(appId, url, vkApi);
                }
                else
                {
                    ShowError("Ошибка", "Неверные параметры");
                    return;
                }
            }

            _viewModel = viewModel;
            DataContext = viewModel;

            await InitializeWebViewAsync();
        }

        private async Task InitializeWebViewAsync()
        {
            if (_viewModel == null) return;

            try
            {
                // Инициализируем WebView2
                await WebView.EnsureCoreWebView2Async();

                // Настраиваем WebView
                await ConfigureWebViewSettings();

                // Получаем сервис и загружаем bridge
                _bridgeService = StaticService.Container.GetRequiredService<VkBridgeService>();
                _bridgeService.Load(_viewModel.AppId, _viewModel.Url, App._host.Services.GetRequiredService<IVkApi>() as VkApi);

                // Настраиваем обработку сообщений
                WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                // Внедряем JavaScript API
                await InjectBridgeJavaScript();

                // Загружаем страницу
                WebView.CoreWebView2.Navigate(_viewModel.Url);
            }
            catch (Exception ex)
            {
                ShowError("Ошибка инициализации WebView", ex.Message);
            }
        }

        private async Task ConfigureWebViewSettings()
        {
            var settings = WebView.CoreWebView2.Settings;

#if !DEBUG
    settings.AreDevToolsEnabled = false;
    settings.IsStatusBarEnabled = false;
    settings.AreDefaultContextMenusEnabled = false;
    settings.AreBrowserAcceleratorKeysEnabled = false;
    settings.AreDefaultScriptDialogsEnabled = false;
#endif

            settings.IsPasswordAutosaveEnabled = false;
            settings.IsGeneralAutofillEnabled = false;
            settings.IsZoomControlEnabled = false;
            settings.IsBuiltInErrorPageEnabled = false;

            settings.UserAgent = "VKAndroidApp/8.154-99999 (Android 12; SDK 32; arm64-v8a; Pixel 6; ru; 2960x1440)";

            // ВАЖНО: Устанавливаем куки для авторизации в VK
            if (AccountsDB.activeAccount.Token != null)
            {
                try
                {
                    var cookieManager = WebView.CoreWebView2.CookieManager;

                    // Основной токен сессии
                    var remixsidCookie = cookieManager.CreateCookie(
                        "remixsid",
                        AccountsDB.activeAccount.Token, // Используем реальный токен
                        ".vk.com",
                        "/"
                    );
                    remixsidCookie.IsHttpOnly = true;
                    remixsidCookie.IsSecure = true;

                    // Токен для мини-приложений
                    var remixappsCookie = cookieManager.CreateCookie(
                        "remixappsid",
                        AccountsDB.activeAccount.Token,
                        ".vk.com",
                        "/"
                    );
                    remixappsCookie.IsSecure = true;

                    // Язык
                    var langCookie = cookieManager.CreateCookie(
                        "remixlang",
                        "0", // 0 - русский, 3 - английский
                        ".vk.com",
                        "/"
                    );

                    // ID пользователя
                    var userCookie = cookieManager.CreateCookie(
                        "remixuid",
                        AccountsDB.activeAccount.id.ToString() ?? "0",
                        ".vk.com",
                        "/"
                    );

                    cookieManager.AddOrUpdateCookie(remixsidCookie);
                    cookieManager.AddOrUpdateCookie(remixappsCookie);
                    cookieManager.AddOrUpdateCookie(langCookie);
                    cookieManager.AddOrUpdateCookie(userCookie);

                    //_log.Debug($"Cookies set for user {_viewModel.VkApi.UserId}");
                }
                catch (Exception ex)
                {
                   // _log.Error($"Failed to set cookies: {ex.Message}");
                }
            }
        }


        private async Task InjectBridgeJavaScript()
        {
            // Внедряем JavaScript для работы с bridge через WebMessage
            var bridgeScript = @"
// Глобальный объект VK для мини-приложений
window.VK = {
    _requestId: 0,
    _callbacks: {},
    _eventListeners: new Map(),
    
    // Основной метод вызова bridge
    call: function(method, params = {}) {
        return new Promise((resolve, reject) => {
            const requestId = 'req_' + (this._requestId++).toString();
            
            // Сохраняем callback
            this._callbacks[requestId] = { resolve, reject };
            
            // Отправляем запрос в C#
            window.chrome.webview.postMessage(JSON.stringify({
                request_id: requestId,
                method: method,
                params: params
            }));
            
            // Таймаут на случай если ответ не придет
            setTimeout(() => {
                if (this._callbacks[requestId]) {
                    delete this._callbacks[requestId];
                    reject(new Error('Request timeout'));
                }
            }, 30000); // 30 секунд таймаут
        });
    },
    
    // Инициализация bridge API
    init: function() {
        // Создаем стандартные методы VK WebApp
        window.VKWebApp = {
            init: (params) => this.call('VKWebAppInit', params),
            getConfig: () => this.call('VKWebAppGetConfig'),
            callAPIMethod: (method, params) => this.call('VKWebAppCallAPIMethod', { method, params }),
            getAuthToken: (params) => this.call('VKWebAppGetAuthToken', params),
            getLaunchParams: () => this.call('VKWebAppGetLaunchParams'),
            storageGet: (keys) => this.call('VKWebAppStorageGet', { keys }),
            storageGetKeys: (params) => this.call('VKWebAppStorageGetKeys', params),
            storageSet: (key, value) => this.call('VKWebAppStorageSet', { key, value }),
            addToCommunity: () => this.call('VKWebAppAddToCommunity'),
            getUserInfo: (params) => this.call('VKWebAppGetUserInfo', params),
            getEmail: () => this.call('VKWebAppGetEmail'),
            getPhoneNumber: () => this.call('VKWebAppGetPhoneNumber'),
            getFriends: (params) => this.call('VKWebAppGetFriends', params),
            getClientVersion: () => this.call('VKWebAppGetClientVersion')
        };
        
        console.log('[VK Bridge] Bridge initialized');
        
        // Получаем поддерживаемые методы
        this.call('SetSupportedHandlers').then(result => {
            console.log('[VK Bridge] Supported methods:', result.supportedHandlers);
        }).catch(err => {
            console.warn('[VK Bridge] Failed to get supported methods:', err);
        });
        
        // Инициализируем приложение
        return window.VKWebApp.init({});
    },
    
    // Подписка на события
    addEventListener: function(event, callback) {
        if (!this._eventListeners.has(event)) {
            this._eventListeners.set(event, []);
        }
        this._eventListeners.get(event).push(callback);
    },
    
    // Отправка события
    _dispatchEvent: function(event, data) {
        const listeners = this._eventListeners.get(event);
        if (listeners) {
            listeners.forEach(callback => {
                try {
                    callback(data);
                } catch (err) {
                    console.error('[VK Bridge] Event listener error:', err);
                }
            });
        }
    }
};

// Обработчик входящих сообщений от C#
window.chrome.webview.addEventListener('message', function(event) {
    try {
        const data = JSON.parse(event.data);
        
        // Обработка ответов на запросы
        if (data.request_id && window.VK._callbacks[data.request_id]) {
            const callback = window.VK._callbacks[data.request_id];
            delete window.VK._callbacks[data.request_id];
            
            if (data.error_type) {
                callback.reject(new Error(data.error_data?.error_reason || 'Unknown error'));
            } else {
                callback.resolve(data.data);
            }
        }
        
        // Обработка событий
        if (data.type && data.type.startsWith('VKWebApp')) {
            window.VK._dispatchEvent(data.type, data.data);
            
            // Также отправляем в стандартный обработчик для React Native WebView
            const rnEvent = new CustomEvent('message', { 
                detail: JSON.stringify(data) 
            });
            document.dispatchEvent(rnEvent);
        }
    } catch (error) {
        console.error('[VK Bridge] Error processing message:', error);
    }
});

// Для совместимости со старым кодом React Native WebView
window.ReactNativeWebView = {
    postMessage: function(message) {
        try {
            const data = JSON.parse(message);
            if (data.handler) {
                return window.VK.call(data.handler, data.params || {});
            }
            return Promise.reject(new Error('Invalid message format'));
        } catch (error) {
            console.error('[VK Bridge] Error in ReactNativeWebView.postMessage:', error);
            return Promise.reject(error);
        }
    }
};

// Автоматически инициализируем bridge при загрузке страницы
document.addEventListener('DOMContentLoaded', function() {
    console.log('[VK Bridge] DOM loaded, initializing bridge...');
    setTimeout(() => {
        window.VK.init().then(() => {
            console.log('[VK Bridge] Bridge initialized successfully');
        }).catch(err => {
            console.error('[VK Bridge] Failed to initialize bridge:', err);
        });
    }, 500);
});

// Также инициализируем если страница уже загружена
if (document.readyState === 'complete' || document.readyState === 'interactive') {
    setTimeout(() => {
        window.VK.init().catch(err => {
            console.error('[VK Bridge] Auto-init error:', err);
        });
    }, 100);
}

console.log('[VK Bridge] JavaScript bridge loaded');
";

            await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(bridgeScript);

            // Добавляем скрипт для передачи launch parameters
            var launchParamsScript = @"
// Функция для вставки launch параметров в глобальную область видимости
function injectLaunchParams(params) {
    if (window.__VKLaunchParams) return;
    
    window.__VKLaunchParams = params;
    window.vkConnect = {
        send: function(method, params) {
            return new Promise((resolve, reject) => {
                window.VK.call(method, params).then(resolve).catch(reject);
            });
        },
        subscribe: function(event, callback) {
            window.VK.addEventListener(event, callback);
        },
        unsubscribe: function(event, callback) {
            // TODO: implement unsubscribe
        }
    };
    
    console.log('[VK Bridge] Launch params injected:', params);
}

// Если есть параметры в URL, парсим их
try {
    const urlParams = new URLSearchParams(window.location.search);
    const vkParams = urlParams.get('vk-params');
    if (vkParams) {
        const params = JSON.parse(decodeURIComponent(vkParams));
        injectLaunchParams(params);
    }
} catch (err) {
    console.warn('[VK Bridge] Failed to parse URL params:', err);
}

// Запрашиваем launch params у bridge
setTimeout(() => {
    if (!window.__VKLaunchParams) {
        window.VK.call('VKWebAppGetLaunchParams').then(params => {
            injectLaunchParams(params);
        }).catch(err => {
            console.warn('[VK Bridge] Failed to get launch params:', err);
        });
    }
}, 1000);
";

            await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(launchParamsScript);

            // Добавляем скрипт для поддержки старого формата запросов (ReactNativeWebView)
            var legacySupportScript = @"
// Поддержка старого формата запросов (для React Native WebView)
const oldBridge = window.chrome.webview.hostObjects.bridge;

// Копируем все методы из bridge для совместимости
if (oldBridge) {
    // Получаем список всех методов
    const methodNames = [
        'VKWebAppInit', 'VKWebAppGetConfig', 'VKWebAppCallAPIMethod',
        'VKWebAppAddToCommunity', 'VKWebAppGetAuthToken', 'VKWebAppGetLaunchParams',
        'VKWebAppStorageGet', 'VKWebAppStorageGetKeys', 'VKWebAppStorageSet',
        'VKWebAppGetUserInfo', 'VKWebAppGetEmail', 'VKWebAppGetPhoneNumber',
        'VKWebAppGetFriends', 'VKWebAppGetClientVersion', 'SetSupportedHandlers'
    ];
    
    // Добавляем метод SetNextRequestId
    if (!oldBridge.SetNextRequestId) {
        oldBridge.SetNextRequestId = function(requestId) {
            // Этот метод вызывается из старого JS кода
            console.log('[Legacy Bridge] SetNextRequestId called:', requestId);
            
            // Мы можем сохранить requestId в глобальной переменной
            window.__legacyRequestId = requestId;
            
            // Или сразу вызвать метод через VK API
            window.VK && window.VK.call('SetNextRequestId', { request_id: requestId })
                .catch(err => console.warn('[Legacy Bridge] Failed to set request id:', err));
        };
    }
}

// Для максимальной совместимости
if (!window.chrome.webview.hostObjects.sync.bridge) {
    window.chrome.webview.hostObjects.sync.bridge = {
        SetNextRequestId: function(requestId) {
            console.log('[Sync Bridge] SetNextRequestId called:', requestId);
            window.__legacyRequestId = requestId;
        }
    };
}

console.log('[VK Bridge] Legacy compatibility layer loaded');
";

            await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(legacySupportScript);

        }

        private async void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = e.TryGetWebMessageAsString();
                if (string.IsNullOrEmpty(message) || _bridgeService == null)
                    return;

                var json = JsonNode.Parse(message)?.AsObject();
                if (json == null)
                    return;

                var requestId = json["request_id"]?.ToString();
                var method = json["method"]?.ToString();
                var parameters = json["params"]?.AsObject() ?? new JsonObject();

                if (string.IsNullOrEmpty(method))
                    return;

                // Обрабатываем запрос через bridge service
                var response = await _bridgeService.HandleBridgeCallAsync(method, parameters, requestId);

                // Отправляем ответ обратно в JavaScript
                var responseJson = JsonSerializer.Serialize(response);
                WebView.CoreWebView2.PostWebMessageAsString(responseJson);
            }
            catch (Exception ex)
            {
                
            }
        }

        private void WebView_OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            InjectBridgeJavaScript();
            _isNavigating = true;
            if (_viewModel != null)
            {
                _viewModel.IsLoading = true;
                _viewModel.HasError = false;
                _viewModel.ErrorMessage = null;
                _viewModel.ErrorDetails = string.Empty;
            }
        }

        private void WebView_OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            _isNavigating = false;
            if (_viewModel != null)
            {
                _viewModel.IsLoading = false;

                if (!args.IsSuccess && args.WebErrorStatus != CoreWebView2WebErrorStatus.OperationCanceled)
                {
                    ShowError("Ошибка при загрузке страницы",
                        args.HttpStatusCode == 0
                            ? args.WebErrorStatus.ToString()
                            : $"Код ошибки: {args.HttpStatusCode}");
                }
            }
        }

        private void ShowError(string message, string details)
        {
            if (_viewModel != null)
            {
                _viewModel.HasError = true;
                _viewModel.ErrorMessage = message;
                _viewModel.ErrorDetails = details;
                _viewModel.IsLoading = false;
            }
        }

        private async void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.HasError = false;
                _viewModel.IsLoading = true;
                await InitializeWebViewAsync();
            }
        }

        private void MiniAppView_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WebView.CoreWebView2 != null)
                {
                    WebView.CoreWebView2.WebMessageReceived -= OnWebMessageReceived;
                }
                WebView.Close();
                _bridgeService?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unloading MiniAppView: {ex.Message}");
            }
        }
    }
}