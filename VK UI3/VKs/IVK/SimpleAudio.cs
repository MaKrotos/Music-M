using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model;

namespace VK_UI3.VKs.IVK
{
    public class SimpleAudio : IVKGetAudio
    {
        public SimpleAudio(DispatcherQueue dispatcher) : base(dispatcher)
        {
        }

        public override long? getCount()
        {
            return listAudio.Count();
        }

        User user;
        public override string getName()
        {
            return null;
        }
        public override List<string> getPhotosList()
        {
            return null;
        }

        public override Uri getPhoto()
        {
            return null;
        }

        public override void GetTracks()
        {
            itsAll = true;
            return;
        }
    }
}
