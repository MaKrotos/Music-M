using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace VK_UI3.Helpers
{
    public class ShortcutManager
    {
        public static async Task<bool> CreateDesktopShortcutAsync()
        {
            try
            {
                var package = Package.Current;
                var appName = package.DisplayName;
                var appUserModelId = package.Id.FamilyName + "!App";

                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var shortcutPath = Path.Combine(desktopPath, $"{appName}.lnk");

                if (File.Exists(shortcutPath))
                {
                    return true;
                }

                var success = await CreateShortcutUsingPowerShellAsync(shortcutPath, appUserModelId, appName);

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания ярлыка: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> CreateShortcutUsingPowerShellAsync(string shortcutPath, string appUserModelId, string appName)
        {
            try
            {
                var psCommand = $@"
try {{
    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut('{shortcutPath}')
    $Shortcut.TargetPath = 'shell:AppsFolder\{appUserModelId}'
    $Shortcut.Description = '{appName}'
    $Shortcut.Save()
    Write-Host 'SUCCESS'
}} catch {{
    Write-Host 'ERROR: ' + $_.Exception.Message
    exit 1
}}";

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{psCommand}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    
                    return process.ExitCode == 0 && output.Contains("SUCCESS");
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsDesktopShortcutExistsAsync()
        {
            try
            {
                var package = Package.Current;
                var appName = package.DisplayName;
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var shortcutPath = Path.Combine(desktopPath, $"{appName}.lnk");

                return File.Exists(shortcutPath);
            }
            catch
            {
                return false;
            }
        }
    }
} 