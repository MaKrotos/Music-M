using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace VK_UI3.Helpers
{
    public class StartupManager
    {
        private const string AppId = "VK M";

        public static async Task<bool> IsAppInStartupAsync()
        {
            var startupTask = await StartupTask.GetAsync(AppId);
            return startupTask.State == StartupTaskState.Enabled;
        }

        public static async Task<bool> EnableStartupAsync()
        {
            var startupTask = await StartupTask.GetAsync(AppId);
            if (startupTask.State != StartupTaskState.Enabled)
            {
                await startupTask.RequestEnableAsync();
            }
            return startupTask.State == StartupTaskState.Enabled;
        }

        public static async Task<bool> DisableStartupAsync()
        {
            var startupTask = await StartupTask.GetAsync(AppId);
            if (startupTask.State == StartupTaskState.Enabled)
            {
                startupTask.Disable();
            }
            return startupTask.State == StartupTaskState.Disabled;
        }
    }
}
