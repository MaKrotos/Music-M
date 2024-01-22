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

      

        public ExtendedAudio(Audio audio, UserAudio userAudio)
        {
            Audio = audio;
        //    this.NumberList = NumberList;
        
            this.userAudio = userAudio;
        }

       public  bool PlayThis { get {
                if (NumberInList == null) return false;
                return (NumberInList == userAudio.currentTrack);
            }  
       }

        public Audio Audio { get; set; }

        // public int NumberList { get; set; }

        public long? NumberInList = null;

        public IVKGetAudio userAudio { get; set; }
    }
}
