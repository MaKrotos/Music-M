using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace VK_UI3.Views.ModalsPages
{
    public class SumbitCaptcha : EventArgs { }

    public sealed partial class CaptchaEnter : Page
    {
        public event EventHandler SumbitPressed;
        public string CaptchaUri { get; set; }
        public string RedirectUri { get; set; }
        public bool IsWebCaptcha => !string.IsNullOrEmpty(RedirectUri);
        public TaskCompletionSource<string> Submitted { get; set; } = new();
        public ContentDialog ParentDialog { get; set; }

        public CaptchaEnter()
        {
            InitializeComponent();
            CancelButton.Click += CancelButton_Click;
        }

        public CaptchaEnter(CaptchaEnter captchaEnter)
        {
            Submitted = captchaEnter.Submitted;
            CaptchaUri = captchaEnter.CaptchaUri;
            RedirectUri = captchaEnter.RedirectUri;
            InitializeComponent();
            CancelButton.Click += CancelButton_Click;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsWebCaptcha)
            {
                CaptchaWebView.Visibility = Visibility.Visible;
                CaptchaWebView.NavigationCompleted += CaptchaWebView_NavigationCompleted;
                await InitializeWebView2Async();
            }
            else
            {
                CaptchaImage.Visibility = Visibility.Visible;
                CodeBox.Visibility = Visibility.Visible;
                ConfirmButton.Visibility = Visibility.Visible;
            }
        }

        private async Task InitializeWebView2Async()
        {
            try
            {
                // Инициализация WebView2
                await CaptchaWebView.EnsureCoreWebView2Async();

                // Настройки WebView2
                CaptchaWebView.CoreWebView2.Settings.IsScriptEnabled = true;
                CaptchaWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;

                // Подписка на события
                CaptchaWebView.WebMessageReceived += CaptchaWebView_WebMessageReceived;
                CaptchaWebView.CoreWebView2.DOMContentLoaded += async (s, e) =>
                {
                    string script = @"
                        (function() {
                            // Перехват fetch для VK ID Captcha
                            const origFetch = window.fetch;
                            window.fetch = async function(...args) {
                                const response = await origFetch.apply(this, args);
                                response.clone().json().then(data => {
                                    if (data && data.response && data.response.success_token) {
                                        window.chrome.webview.postMessage(JSON.stringify({ type: 'success', token: data.response.success_token }));
                                    }
                                }).catch(()=>{});
                                return response;
                            };
                            console.log('fetch перехвачен!');
                            // VK ID Captcha: если есть VKCaptcha, подписываемся на результат
                            if (window.VKCaptcha && typeof window.VKCaptcha.onResult !== 'undefined') {
                                window.VKCaptcha.onResult = function(result) {
                                    if (result && result.key) {
                                        window.chrome.webview.postMessage(JSON.stringify({ type: 'success', token: result.key }));
                                    }
                                };
                            }
                        })();
                    ";
                    await CaptchaWebView.ExecuteScriptAsync(script);
                };

                // Загрузка HTML с капчей
          
                CaptchaWebView.Source = new Uri(RedirectUri);
            }
            catch (Exception ex)
            {
                Submitted.TrySetResult(null);
                ParentDialog?.Hide();
            }
        }

        private void CaptchaWebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = e.TryGetWebMessageAsString();
                var data = JsonDocument.Parse(message).RootElement;
                var type = data.GetProperty("type").GetString();

                switch (type)
                {
                    case "success":
                        string token = null;
                        if (data.TryGetProperty("token", out var tokenProp))
                            token = tokenProp.GetString();
                        else if (data.TryGetProperty("success_token", out var successTokenProp))
                            token = successTokenProp.GetString();
                        else if (data.TryGetProperty("key", out var keyProp))
                            token = keyProp.GetString();
                        Submitted.TrySetResult(token);
                        break;
                    case "close":
                    case "error":
                        Submitted.TrySetResult(null);
                        break;
                }
            }
            catch
            {
                Submitted.TrySetResult(null);
            }
            finally
            {
                ParentDialog?.Hide();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted.TrySetResult(CodeBox.Text);
            ParentDialog?.Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Submitted.TrySetResult(null);
            ParentDialog?.Hide();
        }

        // Добавляем обработку NavigationCompleted для VK ID Captcha
        private void CaptchaWebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                var uri = CaptchaWebView.Source?.ToString();
                if (!string.IsNullOrEmpty(uri) && uri.Contains("success=1") && uri.Contains("key="))
                {
                    var query = new Uri(uri).Query;
                    var parts = query.TrimStart('?').Split('&');
                    string key = null;
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("key="))
                        {
                            key = part.Substring(4);
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(key))
                    {
                        Submitted.TrySetResult(key);
                        ParentDialog?.Hide();
                    }
                }
            }
            catch { /* ignore */ }
        }
    }
}
