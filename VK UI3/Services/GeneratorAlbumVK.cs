using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VK_UI3.Views;
using VK_UI3.Views.Tasks;
using VK_UI3.VKs;
using VkNet.Model.Attachments;

namespace VK_UI3.Services
{
    public class GeneratorAlbumVK : TaskAction
    {
        List<Audio> audios;
        int count = 1000;
        private string name;
        private ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true);

        public GeneratorAlbumVK(List<Audio> audios, int count = 1000, string name = null) : base (audios.Count, "Генерация плейлиста", "genPlayLists")
        {
            this.audios = audios;
            base.total = audios.Count();
            this.count = count;
            this.name = name;
        }

        public async void GenerateAsync()
        {
            try
            {
                var mapRecs = new Dictionary<string, int>();

                foreach (var item in audios)
                {
                    if (base.Status == Statuses.Cancelled) break;

                    pauseEvent.Wait();

                    var audiosRec = await VK.api.Audio.GetRecommendationsAsync(targetAudio: item.FullId, count: 100, offset: 0, shuffle: false);

                    foreach (var track in audiosRec)
                    {
                        if (base.Status == Statuses.Cancelled) break;

                        pauseEvent.Wait();

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
                    audioPlaylist = await VK.api.Audio.CreatePlaylistAsync(DB.AccountsDB.activeAccount.id, "Сгенерированный", audioIds: top1000Tracks);
                }
                else
                {
                    OnCancelled(EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
            }
        }
        AudioPlaylist audioPlaylist = null;
        public override void Cancel()
        {
            base.Status = Statuses.Cancelled;
            OnCancelled(EventArgs.Empty);
        }

        public override void Pause()
        {
            base.Status = Statuses.Pause;
            pauseEvent.Reset();
        }

        public override void Resume()
        {
            base.Status = Statuses.Resume;
            pauseEvent.Set();
        }

        public override void onClick()
        {
            if (base.Status == Statuses.Completed && audioPlaylist != null)
            { MainView.OpenPlayList(audioPlaylist); }
        }
    }

}