using MusicX.Core.Models;
using System.ComponentModel;
using VK_UI3.Controllers;
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

                //if (NumberInList == null) return false;
                // return (NumberInList == iVKGetAudio.currentTrack);
                if (AudioPlayer._TrackDataThis == null) return false;

                if (AudioPlayer._TrackDataThis.audio.AccessKey == this.audio.AccessKey)
                {
                    return true;
                }
                else
                    return false;

               // if (AudioPlayer._TrackDataThis.audio.o)

            }
        }

        public VkNet.Model.Attachments.Audio audio { get; set; }

        // public int NumberList { get; set; }

        public long? _numberInList;
        

        public long? NumberInList { get {
                if (_numberInList == null)
                {
                    iVKGetAudio.updateNumbers();
                }
                return _numberInList;
            
            } set { _numberInList = value;  }
        }

        public IVKGetAudio iVKGetAudio { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
