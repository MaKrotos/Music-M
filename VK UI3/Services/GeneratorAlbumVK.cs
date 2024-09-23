using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.VKs;
using VkNet.Model.Attachments;

namespace VK_UI3.Services
{
    class GeneratorAlbumVK
    {
        List<Audio> audios;
        int count = 1000;

        private string name;

        public GeneratorAlbumVK(List<Audio> audios, int count = 1000, string name = null)
        {
            this.audios = audios;
            this.count = count;
            this.name = name;
        }

        public async Task GenerateAsync()
        {
            try
            {

                var mapRecs = new Dictionary<string, int>();

                foreach (var item in audios)
                {
                    var audiosRec = await VK.api.Audio.GetRecommendationsAsync(targetAudio: item.FullId, count: 100, offset: 0, shuffle: false);

                    foreach (var track in audiosRec)
                    {
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
                }


                var sortedMapRecs = mapRecs.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                IEnumerable<string> top1000Tracks = sortedMapRecs.Keys.Take(this.count);
                var playList = await VK.api.Audio.CreatePlaylistAsync(DB.AccountsDB.activeAccount.id, "Сгенерированный", audioIds: top1000Tracks);
            }
            catch (Exception e)
            { 
            
                
            }

        }
    }
}