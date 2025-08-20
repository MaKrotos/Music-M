using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace VK_UI3.Helpers
{
    public class ShortcutManager
    {
        public static bool IsPackaged()
        {
            try
            {
                return Package.Current != null;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> CreateDesktopShortcutAsync()
        {
            try
            {
                if (IsPackaged())
                {
                    return await CreatePackagedShortcutAsync("desktop");
                }
                else
                {
                    return await CreateUnpackagedShortcutAsync("desktop");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка создания ярлыка: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> CreateStartMenuShortcutAsync()
        {
            try
            {
                if (IsPackaged())
                {
                    return await CreatePackagedShortcutAsync("startmenu");
                }
                else
                {
                    return await CreateUnpackagedShortcutAsync("startmenu");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка создания ярлыка в меню Пуск: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> CreatePackagedShortcutAsync(string shortcutType)
        {
            try
            {
                var package = Package.Current;
                var appName = package.DisplayName;
                var appUserModelId = package.Id.FamilyName + "!App";

                string shortcutPath;
                if (shortcutType == "desktop")
                {
                    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    shortcutPath = Path.Combine(desktopPath, $"{appName}.lnk");
                }
                else // startmenu
                {
                    var startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                    var programsPath = Path.Combine(startMenuPath, "Programs");
                    shortcutPath = Path.Combine(programsPath, $"{appName}.lnk");
                }

                if (File.Exists(shortcutPath))
                {
                    return true;
                }

                var success = await CreateShortcutUsingPowerShellAsync(shortcutPath, appUserModelId, appName, shortcutType);
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка создания упакованного ярлыка: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> CreateUnpackagedShortcutAsync(string shortcutType)
        {
            try
            {
                var appName = "VK Music";
                var appPath = Process.GetCurrentProcess().MainModule.FileName;

                string shortcutPath;
                if (shortcutType == "desktop")
                {
                    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    shortcutPath = Path.Combine(desktopPath, $"{appName}.lnk");
                }
                else // startmenu
                {
                    var startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                    var programsPath = Path.Combine(startMenuPath, "Programs");
                    shortcutPath = Path.Combine(programsPath, $"{appName}.lnk");
                }

                if (File.Exists(shortcutPath))
                {
                    return true;
                }

                var success = await CreateUnpackagedShortcutUsingPowerShellAsync(shortcutPath, appPath, appName, shortcutType);
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка создания неупакованного ярлыка: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> CreateShortcutUsingPowerShellAsync(string shortcutPath, string appUserModelId, string appName, string shortcutType)
        {
            try
            {
                // Создаем директорию если её нет (особенно для Start Menu)
                var directory = Path.GetDirectoryName(shortcutPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var psCommand = $@"
try {{
    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut('{shortcutPath.Replace("'", "''")}')
    $Shortcut.TargetPath = 'explorer.exe'
    $Shortcut.Arguments = 'shell:AppsFolder\{appUserModelId.Replace("'", "''")}'
    $Shortcut.Description = '{appName.Replace("'", "''")}'
    $Shortcut.IconLocation = 'shell:AppsFolder\{appUserModelId.Replace("'", "''")}'
    $Shortcut.WorkingDirectory = '{Environment.GetFolderPath(Environment.SpecialFolder.System).Replace("'", "''")}'
    $Shortcut.Save()
    Write-Host 'SUCCESS'
}} catch {{
    Write-Host 'ERROR: ' + $_.Exception.Message
    exit 1
}}";

                return await ExecutePowerShellCommand(psCommand);
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> CreateUnpackagedShortcutUsingPowerShellAsync(string shortcutPath, string appPath, string appName, string shortcutType)
        {
            try
            {
                // Создаем директорию если её нет
                var directory = Path.GetDirectoryName(shortcutPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var psCommand = $@"
try {{
    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut('{shortcutPath.Replace("'", "''")}')
    $Shortcut.TargetPath = '{appPath.Replace("'", "''")}'
    $Shortcut.Description = '{appName.Replace("'", "''")}'
    $Shortcut.IconLocation = '{appPath.Replace("'", "''")}'
    $Shortcut.WorkingDirectory = '{Path.GetDirectoryName(appPath).Replace("'", "''")}'
    $Shortcut.Save()
    Write-Host 'SUCCESS'
}} catch {{
    Write-Host 'ERROR: ' + $_.Exception.Message
    exit 1
}}";

                return await ExecutePowerShellCommand(psCommand);
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> ExecutePowerShellCommand(string psCommand)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{psCommand.Replace("\"", "\\\"")}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    var output = await outputTask;
                    var error = await errorTask;

                    if (process.ExitCode != 0)
                    {
                        Debug.WriteLine($"PowerShell error: {error}");
                        return false;
                    }

                    return output.Contains("SUCCESS");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PowerShell execution error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> IsDesktopShortcutExistsAsync()
        {
            try
            {
                string shortcutName;

                if (IsPackaged())
                {
                    var package = Package.Current;
                    shortcutName = $"{package.DisplayName}.lnk";
                }
                else
                {
                    shortcutName = "VK Music.lnk";
                }

                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var shortcutPath = Path.Combine(desktopPath, shortcutName);

                return await Task.Run(() => File.Exists(shortcutPath));
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsStartMenuShortcutExistsAsync()
        {
            try
            {
                string shortcutName;

                if (IsPackaged())
                {
                    var package = Package.Current;
                    shortcutName = $"{package.DisplayName}.lnk";
                }
                else
                {
                    shortcutName = "VK Music.lnk";
                }

                var startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                var programsPath = Path.Combine(startMenuPath, "Programs");
                var shortcutPath = Path.Combine(programsPath, shortcutName);

                return await Task.Run(() => File.Exists(shortcutPath));
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> DeleteDesktopShortcutAsync()
        {
            try
            {
                string shortcutName;

                if (IsPackaged())
                {
                    var package = Package.Current;
                    shortcutName = $"{package.DisplayName}.lnk";
                }
                else
                {
                    shortcutName = "VK Music.lnk";
                }

                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var shortcutPath = Path.Combine(desktopPath, shortcutName);

                return await Task.Run(() =>
                {
                    if (File.Exists(shortcutPath))
                    {
                        File.Delete(shortcutPath);
                        return true;
                    }
                    return false;
                });
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> DeleteStartMenuShortcutAsync()
        {
            try
            {
                string shortcutName;

                if (IsPackaged())
                {
                    var package = Package.Current;
                    shortcutName = $"{package.DisplayName}.lnk";
                }
                else
                {
                    shortcutName = "VK Music.lnk";
                }

                var startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                var programsPath = Path.Combine(startMenuPath, "Programs");
                var shortcutPath = Path.Combine(programsPath, shortcutName);

                return await Task.Run(() =>
                {
                    if (File.Exists(shortcutPath))
                    {
                        File.Delete(shortcutPath);
                        return true;
                    }
                    return false;
                });
            }
            catch
            {
                return false;
            }
        }
    }
}