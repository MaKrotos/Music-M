using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Exception;
using VkNet.Model;

namespace VkNet.AudioBypassService.Utils
{
	public static class VkAuthErrors
	{
		/// <summary>
		/// Выбрасывает ошибку, если есть в json.
		/// </summary>
		/// <param name="json"> JSON. </param>
		/// <exception cref="VkApiException">
		/// Неправильные данные JSON.
		/// </exception>
		public static void IfErrorThrowException(string json)
		{
			JObject obj;

			try
			{
				obj = JObject.Parse(json);
			}
			catch (JsonReaderException ex)
			{
				throw new VkApiException("Wrong json data.", ex);
			}

			var error = obj["error"];

			if (error == null || error.Type == JTokenType.Null)
			{
				return;
			}

			if (error.Type != JTokenType.String)
			{
				return;
			}

			var vkAuthError = JsonConvert.DeserializeObject<VkAuthError>(obj.ToString());

			throw VkAuthErrorFactory.Create(vkAuthError);
		}
		
		/// <summary>
		/// Выбрасывает ошибку, если есть в json.
		/// </summary>
		/// <param name="json"> JSON. </param>
		/// <exception cref="VkApiException">
		/// Неправильные данные JSON.
		/// </exception>
		public static void IfErrorThrowException(JObject json)
		{
			var error = json["error"];

			if (error == null || error.Type == JTokenType.Null)
			{
				return;
			}

			if (error.Type != JTokenType.String)
			{
				return;
			}

			var vkAuthError = json.ToObject<VkAuthError>();

			throw VkAuthErrorFactory.Create(vkAuthError);
		}
	}
}