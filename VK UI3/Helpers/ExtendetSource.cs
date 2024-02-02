using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;
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
