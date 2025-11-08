using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ProtoBuf;
using VkNet.AudioBypassService.Models.Google;

namespace VkNet.AudioBypassService.Utils
{
    [UsedImplicitly]
    internal class FakeSafetyNetClient : IDisposable
    {
        private const string GcmUserAgent = "Android-GCM/1.5 (Linux; U; Android 12; Pixel 6 Build/SP2A.220505.006)";
        private const int MaxRetryAttempts = 3;
        private const int RetryDelayMs = 1000;

        private readonly HttpClient _httpClient;

        public FakeSafetyNetClient()
        {
            // 1. Настраиваем обработчик с обходом проверки сертификата (на время тестов)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true, // ⚠️ Только для тестов!
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                Proxy = null,
                UseProxy = false
            };

            // 2. Создаём HttpClient с правильными настройками
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30),
                BaseAddress = new Uri("https://android.clients.google.com")
            };

            // 3. Устанавливаем User-Agent (обходным путём)
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Android-GCM/1.5 (Linux; U; Android 12; Pixel 6 Build/SP2A.220505.006)");

            // 4. Принудительно включаем TLS 1.2/1.3
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            ServicePointManager.DnsRefreshTimeout = 0;
            ServicePointManager.Expect100Continue = false;
        }

        public async Task<AndroidCheckinResponse> CheckIn(int retryCount = MaxRetryAttempts)
        {
            for (int attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    var androidRequest = new AndroidCheckinRequest
                    {
                        Checkin = new AndroidCheckinProto
                        {
                            CellOperator = "310260",
                            Roaming = "mobile:5G:",
                            SimOperator = "310260",
                            Type = DeviceType.DeviceAndroidOs
                        },
                        Digest = "1-929a0dca0eee55513280171a8585da7dcd3700f8",
                        Locale = "ru_RU",
                        LoggingId = -8212629671123625360,
                        Meid = "358240051111110",
                        OtaCerts = { "71Q6Rn2DDZl1zPDVaaeEHItd+Yg=" },
                        TimeZone = "Europe/Moscow",
                        Version = 3
                    };

                    using (var requestStream = new MemoryStream())
                    {
                        Serializer.Serialize(requestStream, androidRequest);
                        var content = new ByteArrayContent(requestStream.ToArray());
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-protobuffer");

                        var response = await _httpClient.PostAsync("https://android.clients.google.com/checkin", content)
                            .ConfigureAwait(false);

                        response.EnsureSuccessStatusCode();

                        using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            return Serializer.Deserialize<AndroidCheckinResponse>(responseStream);
                        }
                    }
                }
                catch (HttpRequestException ex) when (attempt < retryCount)
                {
                    await Task.Delay(RetryDelayMs * attempt).ConfigureAwait(false);
                    continue;
                }
            }

            throw new ApplicationException($"CheckIn failed after {retryCount} attempts");
        }

        public async Task<string> Register(AndroidCheckinResponse credentials, int retryCount = MaxRetryAttempts)
        {
            for (int attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    var requestParams = GetRegisterRequestParams(RandomString.Generate(22), credentials.AndroidId.ToString());

                    using (var content = new FormUrlEncodedContent(requestParams))
                    using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://android.clients.google.com/c2dm/register3"))
                    {
                        httpRequestMessage.Content = content;
                        httpRequestMessage.Headers.TryAddWithoutValidation("Authorization", $"AidLogin {credentials.AndroidId}:{credentials.SecurityToken}");

                        var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                        response.EnsureSuccessStatusCode();

                        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                }
                catch (HttpRequestException ex) when (attempt < retryCount)
                {
                    await Task.Delay(RetryDelayMs * attempt).ConfigureAwait(false);
                    continue;
                }
            }

            throw new ApplicationException($"Register failed after {retryCount} attempts");
        }

        private Dictionary<string, string> GetRegisterRequestParams(string appId, string device)
        {
            return new Dictionary<string, string>
            {
                { "X-scope", "*" },
                { "X-subtype", "841415684880" },
                { "sender", "841415684880" },
                { "X-appid", appId },
                { "app", "com.google.android.gms" },
                { "device", device },
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}