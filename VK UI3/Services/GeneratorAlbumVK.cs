using MusicX.Core.Models.Genius;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Views;
using VK_UI3.Views.Tasks;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;

namespace VK_UI3.Services
{
    public class GeneratorAlbumVK : TaskAction
    {
        List<Audio> audios;
        IVKGetAudio iVKGetAudio;
        int count = 1000;
        private string name;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private TaskCompletionSource<bool> pauseTcs = new TaskCompletionSource<bool>();
        AudioPlaylist audioPlaylist = null;
        private bool isPaused = false;
        private object pauseLock = new object();
        private int deepGen = 1;
        private string description;

        public string CoverPath { get; private set; }

        public GeneratorAlbumVK(List<Audio> audios, string unicId = null, int count = 1000, string name = null, int deepGen = 1, string description = null, bool noDiscover = true, string CoverPath = null) : base(audios.Count, "Генерация плейлиста", unicId, name)
        {
            this.audios = audios;
            base.total = audios.Count();
            this.count = count;
            this.name = name;
            this.deepGen = deepGen;
            this.description = description;
            this.CoverPath = CoverPath;
        }

        public GeneratorAlbumVK(IVKGetAudio audios, string unicId = null, string name = null, int deepGen = 1, string description = null, bool noDiscover = true, string CoverPath = null) : base((int?) (audios.countTracks) ?? 0, "Генерация плейлиста", unicId, name)
        {
            this.iVKGetAudio = audios;
            base.total = (int?)(iVKGetAudio.countTracks) ?? 0;
            this.name = name;
            this.deepGen = deepGen;
            this.description = description;
            this.CoverPath = CoverPath;
        }

        private async Task UploadCoverPlaylist()
        {
            if (!string.IsNullOrEmpty(CoverPath))
            {
                var uploadServer = await VK.vkService.GetPlaylistCoverUploadServerAsync(AccountsDB.activeAccount.id, audioPlaylist.Id);

                var image = await VK.vkService.UploadPhotoToServer(uploadServer, CoverPath);

                await VK.vkService.SetPlaylistCoverAsync((long)audioPlaylist.OwnerId, audioPlaylist.Id, image.Hash, image.Photo);
            }
        }

        public async Task GenerateAsync()
        {
            if (audios != null)
            {
                genByList();
                return;
            }
            if (iVKGetAudio != null)
            {
                getedListAsync();
            }
            return;
        }

        private async Task getedListAsync()
        {
            var need = CalculateTracksToLoad(deepGen, (int) iVKGetAudio.countTracks);

            while (iVKGetAudio.listAudio.Count < need && !iVKGetAudio.itsAll)
            {
                await CheckForPauseAsync();
                var tcs = new TaskCompletionSource<bool>();
                EventHandler handler = null;
                handler = (sender, args) =>
                {
                    iVKGetAudio.onListUpdate -= handler;
                    tcs.SetResult(true);
                };
                iVKGetAudio.onListUpdate += handler;

                iVKGetAudio.GetTracks();
                await tcs.Task;
            }
            audios = new List<Audio>();
            audios.AddRange(iVKGetAudio.listAudio.Select(item => item.audio));
            total = audios.Count();
            this.genByList();
        }

        private int CalculateTracksToLoad(int deepGen, int iVKGetAudioCount)
        {
            int minTracks = 200;
            int maxTracks = 1000;

            // Если количество треков не определено, используем максимум
            if (iVKGetAudioCount == -1)
            {
                iVKGetAudioCount = maxTracks;
            }

           

            // Рассчитываем количество треков для подгрузки
            int tracksToLoad = Math.Min(total, iVKGetAudioCount);

            // Учитываем глубину генерации
            tracksToLoad = (int)(tracksToLoad * (deepGen / 100.0));

            // Убедимся, что количество треков не меньше минимума
            tracksToLoad = Math.Max(tracksToLoad, minTracks);

            return tracksToLoad;
        }


        public async Task genByList()
        {
            try
            {
                if (total == 0) Cancel(); 
                var mapRecs = new Dictionary<string, int>();

                foreach (var item in audios)
                {
                    if (base.Status == Statuses.Cancelled) break;

                    cancellationToken.ThrowIfCancellationRequested();

                    await CheckForPauseAsync();

                    var audiosRec = await VK.api.Audio.GetRecommendationsAsync(targetAudio: item.FullId, count: 100, offset: 0, shuffle: false);

                    foreach (var track in audiosRec)
                    {
                        if (base.Status == Statuses.Cancelled)
                            break;

                        cancellationToken.ThrowIfCancellationRequested();

                        var id = track.Release_audio_id ?? track.FullId;

                        if (audios.Any(a => a.FullId == id || a.Release_audio_id == id))
                        {
                            continue;
                        }

                        if (mapRecs.ContainsKey(id))
                        {
                            mapRecs[id]++;
                        }
                        else
                        {
                            mapRecs[id] = 1;
                        }
                    }

                    Progress++;
                }

                if (base.Status != Statuses.Cancelled)
                {
                    var sortedMapRecs = mapRecs.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                    IEnumerable<string> top1000Tracks = sortedMapRecs.Keys.Take(this.count);

                    audioPlaylist = await VK.api.Audio.CreatePlaylistAsync(DB.AccountsDB.activeAccount.id, name ?? "Сгенерированный", audioIds: top1000Tracks, description: description);
                    await UploadCoverPlaylist();
                    base.Status = Statuses.Completed;
                }
                else
                {
                    OnCancelled(EventArgs.Empty);
                }
            }
            catch (OperationCanceledException)
            {
                base.Status = Statuses.Cancelled;
                OnCancelled(EventArgs.Empty);
            }
            catch (Exception e)
            {
                base.Status = Statuses.Error;
            }
        }

        private async Task CheckForPauseAsync()
        {
            if (isPaused)
            {
                await pauseTcs.Task;
            }
        }

        public override void Cancel()
        {
            base.Status = Statuses.Cancelled;
            cancellationTokenSource.Cancel();
            OnCancelled(EventArgs.Empty);
            delete();
        }

        public override void Pause()
        {
            lock (pauseLock)
            {
                if (!isPaused)
                {
                    isPaused = true;
                    pauseTcs = new TaskCompletionSource<bool>();
                    base.Status = Statuses.Pause;
                }
            }
        }

        public override void Resume()
        {
            lock (pauseLock)
            {
                if (isPaused)
                {
                    isPaused = false;
                    pauseTcs.SetResult(true);
                    base.Status = Statuses.Resume;
                }
            }
        }

        public override void onClick()
        {
            if (base.Status == Statuses.Completed && audioPlaylist != null)
            { MainView.OpenPlayList(audioPlaylist); }
        }
    }

}