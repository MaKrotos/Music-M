using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupLib.Interfaces
{
    public interface IFileDownloadService
    {
        event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
        Task DownloadFileAsync(string url, string destinationPath);
    }
}
