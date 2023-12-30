using System.Security.Cryptography;
using System.Text;

namespace VkNet.AudioBypassService.Utils
{
	internal static class RandomString
	{
		private const string Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";

		public static string Generate(int length)
		{
			var bytes = new byte[length];
			using (var crypto = new RNGCryptoServiceProvider())
			{
				crypto.GetBytes(bytes);
			}

			var result = new StringBuilder(length);
			foreach (var _byte in bytes)
			{
				result.Append(Chars[_byte % Chars.Length]);
			}

			return result.ToString();
		}
	}
}