using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Model.RequestParams.Leads;
using VkNet.Utils;

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
