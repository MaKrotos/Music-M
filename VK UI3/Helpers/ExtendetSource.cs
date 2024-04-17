using VkNet.Model.Attachments;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace VK_UI3.Helpers
{
    internal class ExtendetSource<T> : IMediaPlaybackSource
    {
        private MediaSource mediaSource;

        public ExtendetSource()
        {

        }

        public Audio audio { get; set; }
        public int PosList { get; set; }


    }
}
