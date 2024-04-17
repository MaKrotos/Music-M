using Microsoft.UI.Dispatching;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.VKs;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VK_UI3.Views.Upload
{
    public class UploadTrack
    {
        public event Action<int> OnProgressChanged;
        public EventHandler OnDownloadCompleted;
        private WebClient client;
        private string artist;
        public string name;
        private string? text;
        private int? genreID;
        private bool? scope;
        private DispatcherQueue dispatcherQueue;
        public float percent = 0;


        public static ObservableCollection<UploadTrack> UploadsTracks = new ObservableCollection<UploadTrack>();
        public static EventHandler addedTrack;

        public UploadTrack(DispatcherQueue dispatcherQueue, string pathFile, string name = "Без названия", string artist = "Не указан", string text = null, int? genreID = null, bool? scope = null)
        {
            client = new WebClient();
            this.name = name;
            this.artist = artist;
            client.UploadProgressChanged += Client_UploadProgressChanged;
            client.UploadFileCompleted += Client_UploadFileCompleted;


            this.text = text;
            this.genreID = genreID;
            this.scope = scope;
            this.dispatcherQueue = dispatcherQueue;


            _ = startDownloadAsync(pathFile, name, artist);

        }

        private async void Client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
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
                var resp = await VK.api.CallAsync("audio.save", saveParams);
                Audio audio = JsonConvert.DeserializeObject<Audio>(resp.RawJson);


                if (text != null || genreID != null || scope != null)
                {
                    var param = new AudioEditParams()
                    {
                        OwnerId = (long)audio.OwnerId,
                        AudioId = (long)audio.Id,
                    };
                    if (text != null)
                    {
                        param.Text = text;
                    }
                    if (genreID != null)
                        param.GenreId = (VkNet.Enums.AudioGenre)genreID;
                    if (scope != null)
                        param.NoSearch = scope;

                    VK.api.Audio.EditAsync(param);
                    var ids = new string[] { audio.OwnerId + "_" + audio.Id };
                    audio = (await VK.api.Audio.GetByIdAsync(ids)).ToList()[0];

                }

                dispatcherQueue.TryEnqueue(async () =>
                {
                    UploadsTracks.Remove(this);
                });
                addedTrack?.Invoke(audio, null);
                OnDownloadCompleted?.Invoke(audio, null);
            }
            else
            {
                // Handle the error
                Console.WriteLine($"File upload failed. Error: {e.Error.Message}");
            }
        }

        private void Client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            percent = e.ProgressPercentage;
            OnProgressChanged?.Invoke(e.ProgressPercentage);
        }

        public async Task startDownloadAsync(string pathFile, string name, string artist)
        {
            dispatcherQueue.TryEnqueue(async () =>
            {
                UploadsTracks.Add(this);
            });


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
