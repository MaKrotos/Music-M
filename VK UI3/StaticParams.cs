using System;
using Windows.Foundation.Metadata;

namespace VK_UI3
{
    internal class StaticParams
    {
        public static readonly string tokenStatSly = "i5QlRbdyTpWgWgiNDBysitL88xUswcWAfQSFVWwxj5pwMdcl7KrNBfK0Qk9r";
    }

    public class VKMStatSly : StatSlyLib.StatSLY
    {
        public  static string Token { get; set; } = StaticParams.tokenStatSly;
        public VKMStatSly() : base(Token) 
        {
        }
    }
}