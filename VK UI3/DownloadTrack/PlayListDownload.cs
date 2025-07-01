using FFmpeg.AutoGen;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;

namespace VK_UI3.DownloadTrack
{

    public class PlayListDownload
    {
        public static ObservableCollection<PlayListDownload> PlayListDownloads = new ObservableCollection<PlayListDownload>();
        public static List<IVKGetAudio> PlayListDownloadsList = new List<IVKGetAudio>();


        CancellationTokenSource cts = new CancellationTokenSource();

        public int downloaded = 0;
        public string path;
        private ManualResetEvent pauseEvent = new ManualResetEvent(true);
        private bool pause;
        public IVKGetAudio iVKGetAudio;

        public EventHandler OnTrackDownloaded;
        public void TrackDownloaded() { OnTrackDownloaded?.Invoke(this, EventArgs.Empty); }



        public EventHandler onStatusUpdate;
        public void StatusUpdate() { onStatusUpdate?.Invoke(this, EventArgs.Empty); }




        public static EventHandler OnEndAllDownload;
        public void EndAllDownload() { OnEndAllDownload?.Invoke(this, EventArgs.Empty); }

        public string location;

        public bool isPause()
        {
            return pause;
        }

        DispatcherQueue dispatcherQueue;
        public bool error = false;

        /// <summary>
        /// Создаёт загрузчик
        /// </summary>
        /// <param name="iVKGetAudio">iVKGetAudio отвечающий за логику скачиваний плэйлсита</param>
        /// <param name="path">Путь куда качать плейлист</param>
        public PlayListDownload(IVKGetAudio iVKGetAudio, string path, DispatcherQueue dispatcherQueue, bool disabeLoc = false)
        {
            this.iVKGetAudio = iVKGetAudio;
            this.dispatcherQueue = dispatcherQueue;
            if (iVKGetAudio is SectionAudio) return;
            if (PlayListDownloadsList.Contains(iVKGetAudio)) 
                return;



            foreach (char c in Path.GetInvalidPathChars())
            {
                path = path.Replace(c.ToString(), "");
            }
            this.path = path;

            string? name = iVKGetAudio.name;

            if (name != null)
            {
                string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                foreach (char c in invalidChars)
                {
                    name = name.Replace(c.ToString(), "");

                }
            }
            if (!disabeLoc && name != null)
                this.location = path + "\\" + name;
            else
                this.location = path;


            if (!new CheckFFmpeg().IsExist())
            {
                new DownloadFileWithProgress();
                this.Pause();
            }
            else
            {
                MainWindow.mainWindow.MainWindow_showDownload();
            }

            dispatcherQueue.TryEnqueue(() =>
            {
                PlayListDownloads.Add(this);

                PlayListDownloadsList.Add(iVKGetAudio);
                if (SettingsTable.GetSetting("downloadALL") == null && PlayListDownloads.IndexOf(this) != 0)
                    Pause();
                PlayListDownloadAsync();
            });
        }

        public async Task PlayListDownloadAsync()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    string appPath = AppDomain.CurrentDomain.BaseDirectory;

                    while (iVKGetAudio.listAudio.Count() > downloaded || !iVKGetAudio.itsAll)
                    {
                        pauseEvent.WaitOne();
                        if (cts.Token.IsCancellationRequested)
                        {
                            break;
                        }

                        while (downloaded >= iVKGetAudio.listAudio.Count && iVKGetAudio.countTracks != -1 && !iVKGetAudio.itsAll)
                        {
                            var tcsz = new TaskCompletionSource<bool>();
                            iVKGetAudio.tcs.Add(tcsz);
                            iVKGetAudio.GetTracks();
                            await tcsz.Task;
                        }
                        if (downloaded >= iVKGetAudio.listAudio.Count) break;

                        var a = iVKGetAudio.listAudio[downloaded].audio;

                        if (a.Url == null)
                        {
                            downloaded++;
                            error = true;
                            StatusUpdate();
                            continue;
                        }

                        string input = a.Url.ToString();
                        var invalidChars = Path.GetInvalidFileNameChars();
                        var titl = $"{a.Title}-{a.Artist}.mp3";
                        titl = new string(titl.Where(ch => !invalidChars.Contains(ch)).ToArray());
                        string output = Path.Combine(location, titl);

                        if (File.Exists(output))
                        {
                            downloaded++;
                            StatusUpdate();
                            continue;
                        }

                        var titlTemp = $"{a.OwnerId}_{a.Id}.mp3";
                        string outputTemp = Path.Combine(location, titlTemp);
                        if (File.Exists(outputTemp))
                        {
                            File.Delete(outputTemp);
                        }
                        if (!Directory.Exists(location)) Directory.CreateDirectory(location);

                        // Используем FFmpeg.Autogen для загрузки и конвертации
                        await FFMediaToolkit.FFmpegLoader.DownloadAndConvertWithFFmpegAutogen(input, outputTemp);

                        MakeTags(outputTemp, a);
                        File.Move(outputTemp, output);

                        downloaded++;

                        dispatcherQueue.TryEnqueue(async () =>
                        {
                            OnTrackDownloaded?.Invoke(this, new TrackDownloadedEventArgs { Downloaded = downloaded, Total = (int?)iVKGetAudio.countTracks });
                        });
                    }

                    if (SettingsTable.GetSetting("downloadALL") == null)
                    {
                        var index = PlayListDownloads.IndexOf(this) + 1;
                        if (index <= PlayListDownloads.Count - 1)
                        {
                            PlayListDownloads[index].Resume();
                        }
                    }

                    dispatcherQueue.TryEnqueue(async () =>
                    {
                        PlayListDownloads.Remove(this);
                        PlayListDownloadsList.Remove(iVKGetAudio);
                        if (PlayListDownloads.Count == 0)
                            EndAllDownload();
                    });
                }
                catch (Exception ex)
                {
                    this.error = true;
                    dispatcherQueue.TryEnqueue(async () =>
                    {
                        StatusUpdate();
                    });
                }
            }, cts.Token);
        }

        

        public static void ResumeOnlyFirst()
        {
            foreach (var item in PlayListDownloads)
            {
                item.Pause();
            }
            if (PlayListDownloads.Count != 0)
                PlayListDownloads[0].Resume();
        }

        public static void ResumeAll()
        {
            foreach (var item in PlayListDownloads)
            {
                item.Resume();
            }
        }



        public string MakeTags(string filePath, Audio audio)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File not found at path {filePath}");
                    return filePath;
                }

                var audioFile = TagLib.File.Create(filePath);

                audioFile.Tag.Title = audio.Title != null ? audio.Title : "";
                if (audio.Artist != null)
                {
                    audioFile.Tag.Performers = new[] { audio.Artist };
                }
                audioFile.Tag.Album = audio.Album?.Title != null ? audio.Album.Title : "";
                audioFile.Tag.Track = audio.Id != null ? (uint)audio.Id : 0;
                audioFile.Tag.Year = audio.Date != null ? (uint)((DateTime)audio.Date).Year : 0;
                audioFile.Tag.Comment = audio.Subtitle != null ? audio.Subtitle : "";

                audioFile.Save();

                string newFileName = $"{audio.Title} - {audio.Artist}.mp3";
                string folderPath = Path.GetDirectoryName(filePath);
                string newFilePath = System.IO.Path.Combine(folderPath, newFileName);

                return newFilePath;
            }
            catch { return null; }
        }


        public void Pause()
        {
            pauseEvent.Reset();
            pause = true;

            dispatcherQueue.TryEnqueue(async () =>
            {
                StatusUpdate();
            });
        }

        public void Resume()
        {
           // if (MainWindow.downloadFileWithProgress != null) 
           //     return;
            pauseEvent.Set();
            pause = false;

            dispatcherQueue.TryEnqueue(async () =>
            {
                StatusUpdate();
            });

        }

        public void Cancel()
        {
            if (error)
            {
                PlayListDownloads.Remove(this);
                PlayListDownloadsList.Remove(iVKGetAudio);
                return;
            }

            cts.Cancel();
            pause = true;
            pauseEvent.Set();

            dispatcherQueue.TryEnqueue(async () =>
            {
                StatusUpdate();
            });
        }


    }

    public class TrackDownloadedEventArgs : EventArgs
    {
        public int Downloaded { get; set; }
        public int? Total { get; set; }
    }

}
