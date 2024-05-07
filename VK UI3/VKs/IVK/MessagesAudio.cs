using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VK_UI3.VKs.IVK
{
    public class MessagesAudio : IVKGetAudio
    {
        public MessagesAudio(long id, DispatcherQueue dispatcher) : base(id, dispatcher)
        {
            base.id = id.ToString();
        }

        public override long? getCount()
        {
            return api.Audio.GetCountAsync(long.Parse(id)).Result;
        }

        public User user;
        public override string getName()
        {
            try
            {
                List<long> ids = new List<long> { long.Parse(id) };
                user = api.Users.GetAsync(ids).Result[0];

                var request = new VkParameters
                {
                    {"user_ids", string.Join(",", ids)},
                    {"fields", string.Join(",", "photo_max_orig", "status")}
                };

                var response = api.Call("users.get", request);
                user = User.FromJson(response[0]);


                return user.FirstName + " " + user.LastName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }
        public override List<string> getPhotosList()
        {
            if (photoUri == null) { return new List<string>(); }
            else return new List<string>() { photoUri.ToString() };
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
                int count = 250;

                if (countTracks > listAudio.Count)
                {
                    VkCollection<Audio> audios;


                    audios = api.Audio.GetAsync(new AudioGetParams
                    {
                        OwnerId = int.Parse(id),
                        Offset = offset,
                        Count = count
                    }).Result;


                    ManualResetEvent resetEvent = new ManualResetEvent(false);

                    foreach (var item in audios)
                    {
                        ExtendedAudio extendedAudio = new ExtendedAudio(item, this);

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            listAudio.Add(extendedAudio);
                            resetEvent.Set(); // —игнализирует о завершении задачи
                        });

                        resetEvent.WaitOne(); // ќжидает сигнала о завершении задачи
                        resetEvent.Reset(); // —брасывает событие дл€ следующей итерации
                    }

                    if (countTracks == listAudio.Count()) itsAll = true;


                    getLoadedTracks = false;
                }
                NotifyOnListUpdate();
            });
        }

    }
}
