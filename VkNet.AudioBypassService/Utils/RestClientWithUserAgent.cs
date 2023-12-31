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
			{ "User-Agent", "VKAndroidApp/8.50-17564 (Android 12; SDK 32; arm64-v8a; VK M; ru; 2960x1440)" },
			{ "X-VK-Android-Client", "new" },
			// { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36" },
			// { "Referer", "https://id.vk.com/" },
			// { "Origin", "https://id.vk.com" },
			// { "Sec-Fetch-Mode", "cors" },
			// { "Sec-Fetch-Dest", "empty" },
			// { "Sec-Fetch-Site", "same-site" },
			// { "Sec-Ch-Ua-Platform", "\"Windows\"" },
			// { "Sec-Ch-Ua-Mobile", "?0" },
			// { "Sec-Ch-Ua", "\"Google Chrome\";v=\"117\", \"Not;A=Brand\";v=\"8\", \"Chromium\";v=\"117\"" },
			// { "X-Quic", "1" }
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