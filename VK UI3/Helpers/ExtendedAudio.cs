using MusicX.Core.Models;
using System.ComponentModel;
using VK_UI3.VKs.IVK;


namespace VK_UI3.Helpers
{
    public class ExtendedAudio : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
