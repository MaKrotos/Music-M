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
using Windows.ApplicationModel.UserDataTasks;

namespace VK_UI3.VKs.IVK
{
    public class Recommendations : IVKGetAudio
    {
        public Recommendations(long User_id, DispatcherQueue dispatcher) : base(User_id, dispatcher)
        {
            base.id = User_id.ToString();
        }
        public Recommendations(string targetAudio, DispatcherQueue dispatcher) :base (dispatcher)
        {
           this.targetAudio = targetAudio;
        }
        string targetAudio;

        public override long? getCount()
        {
            return null;
        }

        public User user;
        public override string getName()
        {
            
            return null;
        }
        public override List<string> getPhotosList()
        {
            return new List<string>() { photoUri.ToString() };
        }

        public override Uri getPhoto()
        {
            return null;
        }

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public override void GetTracks()
        {
            if (getLoadedTracks) return;
            getLoadedTracks = true;

            task = Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    uint? offset = (uint?)listAudio.Count;
                    uint? count = 250;

                    VkCollection<Audio> audios;
                    if (string.IsNullOrEmpty(targetAudio))
                    {
                        audios = await VK.api.Audio.GetRecommendationsAsync(userId: long.Parse(base.id), count: count, offset: offset, shuffle: false);
                    }
                    else
                    {
                        audios = await VK.api.Audio.GetRecommendationsAsync(targetAudio: targetAudio, count: count, offset: offset, shuffle: false);
                    }

                    ManualResetEvent resetEvent = new ManualResetEvent(false);

                    foreach (var item in audios)
                    {
                        ExtendedAudio extendedAudio = new ExtendedAudio(item, this);

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            listAudio.Add(extendedAudio);
                            resetEvent.Set();
                        });

                        resetEvent.WaitOne();
                        resetEvent.Reset();
                    }

                    if (countTracks == listAudio.Count()) itsAll = true;

                    getLoadedTracks = false;
                    NotifyOnListUpdate();
                }
                finally
                {
                    semaphore.Release();
                }

                task = null;
            });
        }


    }
}
