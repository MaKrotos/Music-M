using MusicX.Core.Models;
using VK_UI3.Interfaces;


namespace VK_UI3.Helpers
{
    public class ExtendedAudio
    {
        public ExtendedAudio(Audio audio, IVKGetAudio iVKGetAudio)
        {
            this.audio = audio;
            this.iVKGetAudio = iVKGetAudio;
        }
        public ExtendedAudio(VkNet.Model.Attachments.Audio audio, IVKGetAudio iVKGetAudio)
        {
            this.audio = audio;
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

        public VkNet.Model.Attachments.Audio audio { get; set; }

        // public int NumberList { get; set; }

        public long? NumberInList = null;

        public IVKGetAudio iVKGetAudio { get; set; }
    }
}
