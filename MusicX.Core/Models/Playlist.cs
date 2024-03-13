using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model.Attachments;

namespace MusicX.Core.Models
{
    public class Playlist : AudioPlaylist
    {
       

        public string Cover
        {
            get
            {
                if (Photo == null) return null;
                if (Photo.Photo270 != null) return Photo.Photo270;
                if (Photo.Photo135 != null) return Photo.Photo135;
                if (Photo.Photo300 != null) return Photo.Photo300;
                if (Photo.Photo600 != null) return Photo.Photo600;
                if (Photo.Photo1200 != null) return Photo.Photo1200;
                if (Photo.Photo68 != null) return Photo.Photo68;
                return null;


            }
        }
    }
}
