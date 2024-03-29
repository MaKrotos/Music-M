using MusicX.Core.Models;
using System.ComponentModel;
using VK_UI3.Controllers;
using VK_UI3.VKs.IVK;


namespace VK_UI3.Helpers
{
    public class ExtendedAudio
    {
        public ExtendedAudio(VkNet.Model.Attachments.Audio audio, IVKGetAudio iVKGetAudio)
        {
            this.audio = audio;
            this.iVKGetAudio = iVKGetAudio;
        }

        public bool PlayThis
        {
            get
            {
                if (AudioPlayer._TrackDataThis == null) return false;

                return AudioPlayer._TrackDataThis.audio.AccessKey == this.audio.AccessKey;
            }
        }

        public VkNet.Model.Attachments.Audio audio { get; private set; }

        private long? _numberInList;
        public long? NumberInList
        {
            get
            {
                if (_numberInList == null)
                {
                    iVKGetAudio.updateNumbers();
                }
                return _numberInList;
            }
            set
            {
                _numberInList = value;
            }
        }

        public IVKGetAudio iVKGetAudio { get; set; }
    }
}
