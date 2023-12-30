using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using VK_UI3.Helpers;
using VK_UI3.Interfaces;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VK_UI3.VKs
{
    public class UserAudio : IVKGetAudio
    {
        public  UserAudio(long id) : base(id)
        {
         base.id = id;
        }

        public override long getCount()
        {
           return api.Audio.GetCountAsync(base.id).Result;
        }
        User user;

        public override string getName()
        {
            List<long> ids = new List<long> { base.id };
            user = api.Users.GetAsync(ids).Result[0];
            return user.FirstName + " " + user.LastName;
        }

        public override Uri getPhoto()
        {
            if (user != null)
            {
                return
                    user.PhotoMaxOrig;
            }
            else
                return null;
        }

        public override void GetTracks()
        {
            
            int offset = listAudio.Count;
            int count = 6000;

            if (countTracks <= listAudio.Count) return;

                VkCollection<Audio> audios;
                try
                {
                    audios = api.Audio.GetAsync(new AudioGetParams
                    {
                        OwnerId = base.id,
                        Offset = offset,
                        Count = count
                    }).Result;

                    foreach (var item in audios)
                    {
                        ExtendedAudio extendedAudio = new ExtendedAudio(item, this);
                        listAudio.Add(extendedAudio);
                      
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            NotifyOnListUpdate();
        }

      
    }
}
