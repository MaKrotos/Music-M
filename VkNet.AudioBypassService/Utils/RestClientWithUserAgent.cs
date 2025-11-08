using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using VkNet.Abstractions.Utils;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Utils
{
	/// <inheritdoc cref="IRestClient" />
	[UsedImplicitly]
	public class RestClientWithUserAgent : RestClient
	{
		private static readonly IDictionary<string, string> VkHeaders = new Dictionary<string, string>
		{
			{ "User-Agent", "VKAndroidApp/8.154-99999 (Android 12; SDK 32; arm64-v8a; Pixel 6; ru; 2960x1440)" },
			{ "X-VK-Android-Client", "new" },
			{ "Referer", "https://id.vk.ru/" },
			{ "Origin", "https://id.vk.ru" },
			{ "Sec-Fetch-Mode", "cors" },
			{ "Sec-Fetch-Dest", "empty" },
			{ "Sec-Fetch-Site", "same-site" },
			{ "Sec-Ch-Ua-Platform", "\"Android\"" },
			{ "Sec-Ch-Ua-Mobile", "?1" },
			{ "Sec-Ch-Ua", "\"Google Chrome\";v=\"117\", \"Not;A=Brand\";v=\"8\", \"Chromium\";v=\"117\"" },
			{ "X-Quic", "1" }
		};

		public RestClientWithUserAgent(HttpClient httpClient, ILogger<RestClient> logger) : base(httpClient, logger)
		{
			foreach (var header in VkHeaders)
			{
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
			}

			httpClient.DefaultRequestVersion = HttpVersion.Version20;
			httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
		}
	}
}