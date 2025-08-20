using SetupLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SetupLib.Services
{
    public class SystemService : ISystemService
    {
        public string GetOSArchitecture()
        {
            return RuntimeInformation.OSArchitecture switch
            {
                Architecture.Arm64 => "ARM64",
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                _ => throw new Exception("Неизвестная архитектура")
            };
        }

        public Uri GetOSArchitectureURI()
        {
            return RuntimeInformation.OSArchitecture switch
            {
                Architecture.Arm64 => new Uri("https://github.com/MaKrotos/Music-M/releases/download/0.1.0.0/WindowsAppRuntimeInstall.1.6.3-arm64.exe"),
                Architecture.X64 => new Uri("https://github.com/MaKrotos/Music-M/releases/download/0.1.0.0/WindowsAppRuntimeInstall.1.6.3-x64.exe"),
                Architecture.X86 => new Uri("https://github.com/MaKrotos/Music-M/releases/download/0.1.0.0/WindowsAppRuntimeInstall.1.6.3-x86.exe"),
                _ => throw new Exception("Неизвестная архитектура")
            };
        }

        public bool IsAppInstalled(string appName)
        {
            string command = $"if ((Get-AppxPackage).Name -like '*{appName}*') {{ Write-Output \"True\" }} else {{ Write-Output \"False\" }}";
            return ExecutePowerShellCommand(command).Contains("True");
        }

        public bool IsMicrosoftStoreInstalled()
        {
            string command = "if (Get-AppxPackage -Name Microsoft.WindowsStore) { Write-Output \"True\" } else { Write-Output \"False\" }";
            return ExecutePowerShellCommand(command).Contains("True");
        }

        public bool IsDotNetVersionInstalled(string targetVersion)
        {
            try
            {
                var installedVersions = GetInstalledDotNetVersions();
                var targetVersionNumber = GetVersionNumber(targetVersion);

                foreach (var version in installedVersions)
                {
                    var ver = GetVersionNumber(version);
                    if (ver?.Major != targetVersionNumber?.Major)
                        continue;

                    var versionNumber = GetVersionNumber(version);
                    if (versionNumber != null && versionNumber >= targetVersionNumber && version.Contains("WindowsDesktop"))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<string> GetInstalledDotNetVersions()
        {
            var versions = new List<string>();
            versions.AddRange(GetDotNetInfo("dotnet --list-sdks"));
            versions.AddRange(GetDotNetInfo("dotnet --list-runtimes"));
            return versions.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
        }

        public bool CheckIfWingetIsInstalled()
        {
            string output = ExecutePowerShellCommand("winget; echo $?");
            return output.Trim().EndsWith("True");
        }

        public bool IsRunningAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private Version GetVersionNumber(string versionString)
        {
            var match = Regex.Match(versionString, @"(\d+(\.\d+)+)");
            return match.Success ? new Version(match.Value) : null;
        }

        private List<string> GetDotNetInfo(string command)
        {
            return ExecutePowerShellCommand(command)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        private string ExecutePowerShellCommand(string command)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"$OutputEncoding = [Console]::OutputEncoding = [Text.Encoding]::UTF8; {command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }
    }
}
