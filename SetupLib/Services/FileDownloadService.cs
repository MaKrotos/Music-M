using SetupLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupLib.Services
{
    public class FileDownloadService : IFileDownloadService
    {
        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        public async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) })
            using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                using (var streamToWriteTo = File.Create(destinationPath))
                {
                    var totalRead = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await streamToWriteTo.WriteAsync(buffer, 0, read);
                            totalRead += read;

                            var percentage = totalRead * 100d / response.Content.Headers.ContentLength.Value;
                            OnDownloadProgressChanged(
                                response.Content.Headers.ContentLength.Value,
                                totalRead,
                                percentage
                            );
                        }
                    } while (isMoreToRead);

                    await streamToWriteTo.FlushAsync();
                }
            }
        }

        protected virtual void OnDownloadProgressChanged(long totalBytes, long bytesDownloaded, double percentage)
        {
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs
            {
                TotalBytes = totalBytes,
                BytesDownloaded = bytesDownloaded,
                Percentage = percentage
            });
        }
    }
}
