using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Views.Tasks;
using VkNet.Model.RequestParams;
using VkNet.Utils;
namespace VK_UI3.VKs
{
    internal class KrotosVK
    {

  

        public async Task archiveTracksFromProfileAsync(VkNet.Abstractions.IVkApi vkApi)
        {

            static List<List<T>> SplitList<T>(List<T> list, int chunkSize)
            {
                return list
                    .Select((item, index) => new { item, index })
                    .GroupBy(x => x.index / chunkSize)
                    .Select(g => g.Select(x => x.item).ToList())
                    .ToList();
            }


            try
            {
                var offset = 0;

                VkCollection<VkNet.Model.Attachments.Audio> audio;
                List<VkNet.Model.Attachments.Audio> audios = new();
                do
                {
                    AudioGetParams param = new AudioGetParams()
                    {
                        OwnerId = AccountsDB.activeAccount.id,
                        Offset = offset,
                        Count = 6000

                    };
                    offset += 6000;
                    audio = await vkApi.Audio.GetAsync(param);

                    audios.AddRange(audio);



                } while (audio.Count > 0);
                List<List<VkNet.Model.Attachments.Audio>> chunks = SplitList(audios, 1000);

                var i = 1;
                var j = 1;
                var dateString = DateTime.Now.ToString("yyyy MM d");
                


                List<Func<Task>> tasks = new List<Func<Task>>();
                foreach (var item in chunks)
                {
                    tasks.Add(
                        async () =>
                        {
                            var ch = new List<string>();
                            foreach (var aud in item)
                            {
                                ch.Add(aud.FullId);
                            }


                            await vkApi.Audio.CreatePlaylistAsync(DB.AccountsDB.activeAccount.id,
                                $"Архив №{i++} ({dateString})",
                                $"Архив треков со стр ({dateString}) c {j} по {j + ch.Count - 1}",
                                ch
                                );
                            j += 1000;
                        }
                    );
                }

                new TaskListActions(tasks, tasks.Count, "Создаю архивы...", null, null, 1000);






                tasks = new List<Func<Task>>();
         
                foreach (var item in audios)
                {
                    tasks.Add(
                        async () =>
                        {
                            await vkApi.Audio.DeleteAsync((long)item.Id, (long)item.OwnerId);
                        }
                    );
                }

                new TaskListActions(tasks, tasks.Count, "Удаляю треки с профиля...", null, null, 1000);

                
              
            }
            catch
            (Exception e)
            {

            }
        }

        public static async Task sendVKAudioPlayStat(Helpers.ExtendedAudio trackdata, Helpers.ExtendedAudio Pretrackdata = null, int? secDurPre = null)
        {

            var startPlay = new PlayTrackEvent
            {
                Event = "music_start_playback",
                AudioId = trackdata.audio.FullId,
                Uuid = Guid.NewGuid().GetHashCode(),
                StartTime = "0",
                Shuffle = "false",
                Reason = "auto",
                PlaybackStartedAt = "0",
                TrackCode = trackdata.audio.TrackCode,
                Repeat = "all",
                State = "app",
                PlaylistId = trackdata.audio.Album?.ToOwnerIdString()!
            };

            var queue = new List<TrackEvent>(2) { startPlay };

            if (Pretrackdata != null)
            {
                startPlay.PrevAudioId = Pretrackdata.audio.FullId; // ← Исправлено

                queue.Add(new StopTrackEvent
                {
                    Event = "music_stop_playback",
                    Uuid = Guid.NewGuid().GetHashCode(),
                    Shuffle = "false",
                    Reason = "new",
                    AudioId = Pretrackdata.audio.FullId, // ← ИСПРАВЛЕНО: должен быть ID предыдущего трека
                    StartTime = "0",
                    PlaybackStartedAt = "0",
                    TrackCode = Pretrackdata.audio.TrackCode, // ← ИСПРАВЛЕНО: трек-код предыдущего трека
                    StreamingType = "online",
                    Duration = (secDurPre ?? Pretrackdata.audio.Duration).ToString(),
                    Repeat = "all",
                    State = "app",
                    // Source = Pretrackdata.ParentBlockId!,
                    PlaylistId = Pretrackdata.audio.Album?.ToOwnerIdString()!
                });
            }

            await VK.vkService.StatsTrackEvents(queue);
        }

    }
}
