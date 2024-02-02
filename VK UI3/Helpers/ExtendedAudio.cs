using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.Interfaces;
using VK_UI3.VKs;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace VK_UI3.Helpers
{
    public class ExtendedAudio
    {
        public ExtendedAudio(Audio audio, IVKGetAudio iVKGetAudio)
        {
            Audio = audio;
            this.iVKGetAudio = iVKGetAudio;
        }

        public bool PlayThis
        {
            get
            {
                if (NumberInList == null) return false;
                return (NumberInList == iVKGetAudio.currentTrack);
            }
        }

        public Audio Audio { get; set; }

        // public int NumberList { get; set; }

        public long? NumberInList = null;

        public IVKGetAudio iVKGetAudio { get; set; }
    }
}
