using System;
using System.Diagnostics;
using System.Text;

namespace VK_UI3.Helpers;

public static class StringExtensions
{
    /// <summary>
    ///     Decodes Base64 URL encoded string to byte array.
    /// </summary>
    /// <param name="text">Base64 URL encoded string.</param>
    /// <returns>Byte array.</returns>
    public static byte[] Base64UrlDecode(this string? text)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        var base64 = text
            .Replace("-", "+")
            .Replace("_", "/")
            .Replace("=", "")
            .Replace("\r", "")
            .Replace("\n", "");
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        try
        {
            return Convert.FromBase64String(base64);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    ///     Encodes byte array to string using Base64 URL encoding.
    /// </summary>
    /// <param name="data">Byte array.</param>
    /// <returns>Base64 URL encoded string.</returns>
    public static string Base64UrlEncode(this byte[] data)
    {
        var base64 = Convert.ToBase64String(data);
        return base64.TrimEnd('=').Replace("+", "-").Replace("/", "_");
    }

    /// <summary>
    ///     Encodes byte array to string using Base64 URL encoding.
    /// </summary>
    /// <param name="data">Raw String.</param>
    /// <returns>Base64 URL encoded string.</returns>
    public static string Base64UrlEncode(this string data)
    {
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        return base64.TrimEnd('=').Replace("+", "-").Replace("/", "_");
    }
}
