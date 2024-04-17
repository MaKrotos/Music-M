using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ProtoBuf;
using VkNet.AudioBypassService.Models.Google;

namespace VkNet.AudioBypassService.Utils
{
	[UsedImplicitly]
	internal class FakeSafetyNetClient : IDisposable
	{
		private const string GcmUserAgent = "Android-GCM/1.5 (generic_x86 KK)";

		private readonly HttpClient _httpClient;

		public FakeSafetyNetClient()
		{
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(GcmUserAgent);
		}

		public async Task<AndroidCheckinResponse> CheckIn()
		{
			var androidRequest = new AndroidCheckinRequest
			{
				Checkin = new AndroidCheckinProto
				{
					CellOperator = "310260",
					Roaming = "mobile:LTE:",
					SimOperator = "310260",
					Type = DeviceType.DeviceAndroidOs
				},
				Digest = "1-929a0dca0eee55513280171a8585da7dcd3700f8",
				Locale = "en_US",
				LoggingId = -8212629671123625360,
				Meid = "358240051111110",
				OtaCerts =
				{
					"71Q6Rn2DDZl1zPDVaaeEHItd+Yg="
				},
				TimeZone = "America/New_York",
				Version = 3
			};

			var requestStream = new MemoryStream();
			Serializer.Serialize(requestStream, androidRequest);

			var content = new ByteArrayContent(requestStream.ToArray());
			content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-protobuffer");

			var response = await _httpClient.PostAsync("https://android.clients.google.com/checkin", content);

			response.EnsureSuccessStatusCode();

			var responseStream = await response.Content.ReadAsStreamAsync();

			var androidCheckinResponse = Serializer.Deserialize<AndroidCheckinResponse>(responseStream);

			return androidCheckinResponse;
		}

		public async Task<string> Register(AndroidCheckinResponse credentials)
		{
			var requestParams = GetRegisterRequestParams(RandomString.Generate(22), credentials.AndroidId.ToString());

			var content = new FormUrlEncodedContent(requestParams);

			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://android.clients.google.com/c2dm/register3")
			{
				Content = content
			};
			httpRequestMessage.Headers.TryAddWithoutValidation("Authorization", $"AidLogin {credentials.AndroidId}:{credentials.SecurityToken}");

			var response = await _httpClient.SendAsync(httpRequestMessage);

			response.EnsureSuccessStatusCode();

			var registerResponse = await response.Content.ReadAsStringAsync();

			return registerResponse;
		}

		private Dictionary<string, string> GetRegisterRequestParams(string appId, string device)
		{
			return new Dictionary<string, string>
			{
				{ "X-scope", "*" },
				{ "X-subtype", "841415684880" },
				{ "sender", "841415684880" },
				{ "X-appid", appId },
				{ "app", "com.vkontakte.android" },
				{ "device", device },
			};
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			_httpClient?.Dispose();
		}
	}
}