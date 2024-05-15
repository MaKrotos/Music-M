using Microsoft.UI.Dispatching;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VkNet.Model.Attachments;
using VkNet.Utils;

namespace VK_UI3.VKs.IVK
{
    public class MixAudio : IVKGetAudio
    {
        public string mix_id;
        public MixAudio(string mix_id, DispatcherQueue dispatcher) : base(dispatcher)
        {
            this.mix_id = mix_id;
            //Ќу никто ж не будет слушать столько, да?
            this.countTracks = -1;
      
            Task.Run(async () =>
            {
                VkParameters keyValuePairs = new VkParameters();
                keyValuePairs.Add("mix_id", mix_id);
                keyValuePairs.Add("count", 50);

                try
                {
                    var a = await VK.api.CallAsync("audio.getStreamMixAudios", keyValuePairs);
                    List<Audio> audios = JsonConvert.DeserializeObject<List<Audio>>(a.RawJson);
                    foreach (var item in audios)
                    {
                        listAudioTrue.Add(new Helpers.ExtendedAudio(item, this));
                    }
                }
                catch (Exception e)
                {
                }
                this.currentTrack = 0;
                AudioPlayer.PlayList(this);
            });
        }

        public override long? getCount()
        {
            return listAudio.Count();
        }


        public override string getName()
        {
            return null;
        }
        public override List<string> getPhotosList()
        {
            return null;
        }

        public override Uri getPhoto()
        {
            return null;
        }

        public override void GetTracks()
        {
            if (getLoadedTracks) return;
            getLoadedTracks = true;
            Task.Run(async () =>
            {
                VkParameters keyValuePairs = new VkParameters();
                keyValuePairs.Add("mix_id", mix_id);
                keyValuePairs.Add("count", 50);

                try
                {
                    var a = await VK.api.CallAsync("audio.getStreamMixAudios", keyValuePairs);
                    List<Audio> audios = JsonConvert.DeserializeObject<List<Audio>>(a.RawJson);
                    foreach (var item in audios)
                    {
                        listAudioTrue.Add(new Helpers.ExtendedAudio(item, this));
                    }
                }
                catch (Exception e)
                {
                }
                getLoadedTracks = false;
                NotifyOnListUpdate();
            });

        }

    }
}
