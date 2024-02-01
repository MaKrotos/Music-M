using MusicX.Core.Models;
using MusicX.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using VK_UI3.Interfaces;

namespace VK_UI3.VKs
{
    class SectionAudio : IVKGetAudio
    {


        string SectionID;

        public SectionAudio(Block block) : base(block)
        {
        }

        public override long? getCount()
        {
            return null;
        }

        public override string getName()
        {
            return null;
        }

        public override Uri getPhoto()
        {
            return null;
        }

        public override async void GetTracks()
        {
            if (getLoadedTracks) return;
            getLoadedTracks = true;

            var audios = (await VK.vkService.GetSectionAsync(id, Next)).Audios;

            if (audios.Count == 0)
            {
                countTracks = listAudioTrue.Count;
                itsAll = true;
            }
            foreach (var item in audios)
            {
                ExtendedAudio extendedAudio = new ExtendedAudio(item, this);
                listAudioTrue.Add(extendedAudio);

                NotifyOnListUpdate();
            }
            
            getLoadedTracks = false;
        }
    }
}
