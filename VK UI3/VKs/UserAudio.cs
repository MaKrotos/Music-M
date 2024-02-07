using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public UserAudio(long id, DispatcherQueue dispatcher) : base(id, dispatcher)
        {
            base.id = id.ToString();
        }

        public override long? getCount()
        {
            return api.Audio.GetCountAsync(long.Parse(base.id)).Result;
        }
        User user;

        public override string getName()
        {
            try
            {
                List<long> ids = new List<long> { long.Parse(base.id) };
                user = api.Users.GetAsync(ids).Result[0];
                return user.FirstName + " " + user.LastName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
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
            if (getLoadedTracks) return;
            getLoadedTracks = true;

            Task.Run(async () =>
            {
                int offset = listAudio.Count;
                int count = 100;

                if (countTracks > listAudio.Count)
                {
                    VkCollection<Audio> audios;

                    audios = api.Audio.GetAsync(new AudioGetParams
                    {
                        OwnerId = int.Parse(base.id),
                        Offset = offset,
                        Count = count
                    }).Result;


                    foreach (var item in audios)
                    {
                        ExtendedAudio extendedAudio = new ExtendedAudio(item, this);
                        ManualResetEvent resetEvent = new ManualResetEvent(false);

                        this.DispatcherQueue.TryEnqueue(() =>
                        {
                            listAudio.Add(extendedAudio);
                            resetEvent.Set();
                        });

                        resetEvent.WaitOne();
                    }


                    if (countTracks == listAudio.Count()) itsAll = true;

                  
                    getLoadedTracks = false;
                }
                NotifyOnListUpdate();
            });
        }

    }
}
