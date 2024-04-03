using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.VKs;
using VkNet.Utils;

namespace VK_UI3.Views.Upload
{
    class UploadTrack
    {
        public event Action<int> OnProgressChanged;
        public event Action OnDownloadCompleted;
        private WebClient client;
        private string artist;
        private string name;

        public static ObservableCollection<UploadTrack> Tracks = new ObservableCollection<UploadTrack>();

        public UploadTrack(string pathFile, string name = "Без названия", string artist = "Не указан")
        {
            client = new WebClient();
            this.name = name;
            this.artist = artist;
            client.UploadProgressChanged += Client_UploadProgressChanged;
            client.UploadFileCompleted += Client_UploadFileCompleted;
            _ = startDownloadAsync(pathFile, name, artist);
        }

        private void Client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                // Handle the response from the server
                var uploadResponse = Encoding.UTF8.GetString(e.Result);
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(uploadResponse);

                // Save the audio file using the 'audio.save' method
                var saveParams = new VkParameters
                    {
                        {"server", jsonResponse["server"]},
                        {"audio", jsonResponse["audio"]},
                        {"hash", jsonResponse["hash"]},
                        {"artist", artist},
                        {"title", name}
                    };
                VK.api.CallAsync("audio.save", saveParams);
                Console.Write(saveParams);

                // Вызовите событие завершения загрузки
                OnDownloadCompleted?.Invoke();
            }
            else
            {
                // Handle the error
                Console.WriteLine($"File upload failed. Error: {e.Error.Message}");
            }
        }

        private void Client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            OnProgressChanged?.Invoke(e.ProgressPercentage);
        }

        public async Task startDownloadAsync(string pathFile, string name, string artist)
        {
            var uploadUrl = await VK.getUploadServerAsync();

            if (pathFile != null && uploadUrl != null)
            {
                client.UploadFileAsync(new Uri(uploadUrl), pathFile);
            }
            else
            {
                // Handle the case where the file or uploadUrl is null
            }
        }

        // Метод отмены
        public void CancelDownload()
        {
            client.CancelAsync();
        }
    }
}
