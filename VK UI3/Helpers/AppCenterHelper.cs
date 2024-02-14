using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.Services;

namespace VK_UI3.Helpers
{
    internal class AppCenterHelper
    {
        public static async Task SendCrash(Exception ex) 
        {
            var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
            Crashes.TrackError(ex, properties);
        }
    }
}
