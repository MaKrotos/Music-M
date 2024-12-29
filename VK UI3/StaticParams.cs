using System;
using Windows.Foundation.Metadata;

namespace VK_UI3
{
    internal class StaticParams
    {
        public static readonly string tokenStatSly = Environment.GetEnvironmentVariable("TOKEN_STAT_SLY");
    }

    public class VKMStatSly : StatSlyLib.StatSLY
    {
        public  static string Token { get; set; } = StaticParams.tokenStatSly;
        public VKMStatSly() : base(Token) 
        {
        }
    }
}
