using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using Windows.ApplicationModel;


namespace VK_UI3.Helpers;
public class StartupManager
{
    private const string AppId = "VK M";
    private const string AppName = "VK M";
    private static readonly string StartupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static async Task<bool> IsAppInStartupAsync()
    {
        if (Packaged.IsPackaged())
        {
            return await IsAppInStartupPackagedAsync();
        }
        else
        {
            return await IsAppInStartupUnpackagedAsync();
        }
    }

    public static async Task<bool> EnableStartupAsync()
    {
        if (Packaged.IsPackaged())
        {
            return await EnableStartupPackagedAsync();
        }
        else
        {
            return await EnableStartupUnpackagedAsync();
        }
    }

    public static async Task<bool> DisableStartupAsync()
    {
        if (Packaged.IsPackaged())
        {
            return await DisableStartupPackagedAsync();
        }
        else
        {
            return await DisableStartupUnpackagedAsync();
        }
    }

    #region Packaged App Methods
    private static async Task<bool> IsAppInStartupPackagedAsync()
    {
        try
        {
            var startupTask = await StartupTask.GetAsync(AppId);
            return startupTask.State == StartupTaskState.Enabled ||
                   startupTask.State == StartupTaskState.EnabledByPolicy;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking packaged startup: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> EnableStartupPackagedAsync()
    {
        try
        {
            var startupTask = await StartupTask.GetAsync(AppId);
            if (startupTask.State != StartupTaskState.Enabled &&
                startupTask.State != StartupTaskState.EnabledByPolicy)
            {
                var result = await startupTask.RequestEnableAsync();
                return result == StartupTaskState.Enabled;
            }
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error enabling packaged startup: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> DisableStartupPackagedAsync()
    {
        try
        {
            var startupTask = await StartupTask.GetAsync(AppId);
            if (startupTask.State == StartupTaskState.Enabled ||
                startupTask.State == StartupTaskState.EnabledByPolicy)
            {
                startupTask.Disable();
                return true;
            }
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error disabling packaged startup: {ex.Message}");
            return false;
        }
    }
    #endregion

    #region Unpackaged App Methods
    private static string GetAppPathWithArguments()
    {
        var appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        // Можно добавить аргументы командной строки если нужно
        // return $"\"{appPath}\" --minimized";
        return $"\"{appPath}\"";
    }

    private static Task<bool> IsAppInStartupUnpackagedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, false))
                {
                    var value = key?.GetValue(AppName) as string;
                    var expectedPath = GetAppPathWithArguments();
                    return !string.IsNullOrEmpty(value) &&
                           value.Equals(expectedPath, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking unpackaged startup: {ex.Message}");
                return false;
            }
        });
    }

    private static Task<bool> EnableStartupUnpackagedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true))
                {
                    if (key == null)
                        return false;

                    key.SetValue(AppName, GetAppPathWithArguments(), RegistryValueKind.String);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enabling unpackaged startup: {ex.Message}");
                return false;
            }
        });
    }

    private static Task<bool> DisableStartupUnpackagedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true))
                {
                    if (key == null)
                        return false;

                    key.DeleteValue(AppName, false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disabling unpackaged startup: {ex.Message}");
                return false;
            }
        });
    }
    #endregion
}