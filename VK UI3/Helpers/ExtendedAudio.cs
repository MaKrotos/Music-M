using System;
using VK_UI3.Controllers;
using VK_UI3.VKs.IVK;


namespace VK_UI3.Helpers
{
    public class ExtendedAudio
    {

        public class SelectedChange : EventArgs
        {
            public bool selected;

            public SelectedChange(bool selected)
            {
                this.selected = selected;
            }
        }

        public event EventHandler trackSelectChanged;

        public void trackSelectChangedInvoke(SelectedChange selectedChange)
        {
            trackSelectChanged.Invoke(iVKGetAudio, selectedChange);
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
                var trackdata = AudioPlayer._TrackDataThisGet().Result;

                if (trackdata == null) return false;

                return trackdata.audio.AccessKey == this.audio.AccessKey;
            }
        }

        public VkNet.Model.Attachments.Audio audio { get; set; }

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
