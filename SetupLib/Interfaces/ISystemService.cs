using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupLib.Interfaces
{

    public interface ISystemService
    {
        string GetOSArchitecture();
        Uri GetOSArchitectureURI();
        bool IsAppInstalled(string appName);
        bool IsMicrosoftStoreInstalled();
        bool IsDotNetVersionInstalled(string targetVersion);
        List<string> GetInstalledDotNetVersions();
        bool CheckIfWingetIsInstalled();
        bool IsRunningAsAdministrator();
    }

}
