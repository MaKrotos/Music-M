using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken cancellationToken;

        public GeneratorAlbumVK(List<Audio> audios, string unicId, int count = 1000, string name = null) : base(audios.Count, "Генерация плейлиста", unicId)
        {
            this.audios = audios;
            base.total = audios.Count();
            this.count = count;
            this.name = name;
            this.cancellationToken = cancellationTokenSource.Token;
        }

        public async Task GenerateAsync()
        {
            try
            {
                var mapRecs = new Dictionary<string, int>();

                foreach (var item in audios)
                {
                    if (base.Status == Statuses.Cancelled) break;

                    cancellationToken.ThrowIfCancellationRequested();

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
                    audioPlaylist = await VK.api.Audio.CreatePlaylistAsync(DB.AccountsDB.activeAccount.id, "Сгенерированный", audioIds: top1000Tracks);
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
        AudioPlaylist audioPlaylist = null;
        public override void Cancel()
        {
            base.Status = Statuses.Cancelled;
            cancellationTokenSource.Cancel();
            OnCancelled(EventArgs.Empty);
            delete();
        }

        public override void Pause()
        {
            base.Status = Statuses.Pause;
            cancellationTokenSource.Cancel();
        }

        public override void Resume()
        {
            base.Status = Statuses.Resume;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            GenerateAsync();
        }

        public override void onClick()
        {
            if (base.Status == Statuses.Completed && audioPlaylist != null)
            { MainView.OpenPlayList(audioPlaylist); }
        }
    }


}