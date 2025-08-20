using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupLib.Interfaces
{
    public interface IGitHubClientService
    {
        Task<ReleaseInfo> GetLatestReleaseInfo(string owner, string repo, string currentVersion);
    }



}
