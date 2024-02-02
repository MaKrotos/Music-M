using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Exception;
using VkNet.Model;

namespace VkNet.AudioBypassService.Utils
{
	public static class VkAuthErrors
	{
		/// <summary>
		/// Р’С‹Р±СЂР°СЃС‹РІР°РµС‚ РѕС€РёР±РєСѓ, РµСЃР»Рё РµСЃС‚СЊ РІ json.
		/// </summary>
		/// <param name="json"> JSON. </param>
		/// <exception cref="VkApiException">
		/// РќРµРїСЂР°РІРёР»СЊРЅС‹Рµ РґР°РЅРЅС‹Рµ JSON.
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
		/// Р’С‹Р±СЂР°СЃС‹РІР°РµС‚ РѕС€РёР±РєСѓ, РµСЃР»Рё РµСЃС‚СЊ РІ json.
		/// </summary>
		/// <param name="json"> JSON. </param>
		/// <exception cref="VkApiException">
		/// РќРµРїСЂР°РІРёР»СЊРЅС‹Рµ РґР°РЅРЅС‹Рµ JSON.
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
