using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupLib.Interfaces
{
    public interface IInstallationService
    {
        event EventHandler<InstallStatusChangedEventArgs> InstallStatusChanged;
        Task InstallUpdateAsync(AppUpdater appUpdater, bool skipCertCheck, bool forceInstall, string? PathInstallZIP = null);
        Task InstallAppInstallerAsync();
        Task InstallCertificateAsync(string certificateUrl);
        Task InstallAppRuntimeAsync();
        void InstallLatestDotNetRuntime();
    }

}
