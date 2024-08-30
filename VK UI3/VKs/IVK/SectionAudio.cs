using Microsoft.UI.Dispatching;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.Helpers;

namespace VK_UI3.VKs.IVK
{
    public class SectionAudio : IVKGetAudio
    {


        string SectionID;

        public SectionAudio(Block block, DispatcherQueue dispatcher) : base(block, dispatcher)
        {
        }

        public SectionAudio(string sectionID, DispatcherQueue dispatcher, Uri photoLink = null, string name = null, List<Audio> audios = null, string next = null) : base(sectionID, dispatcher, photoLink, name, audios, next)
        {

        }

        public override long? getCount()
        {
            return null;
        }

        public override string getName()
        {
            return null;
        }

        public override Uri getPhoto()
        {
            return null;
        }
        private readonly object _lock = new object();
        public override List<string> getPhotosList()
        {
            return new List<string>();
        }
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);
        public override void GetTracks()
        {
            if (getLoadedTracks) 
                return;
            if (itsAll) 
                return;



            ResponseData a = null;

            task = Task.Run(async () =>
            {
                try
                {
                    if (getLoadedTracks)
                        return;
                    getLoadedTracks = true;

                    a = await VK.vkService.GetSectionAsync(id, Next);

                    if (a.Section != null)
                    {
                        if (a.Section.NextFrom == null)
                        {
                            itsAll = true;

                        }
                        Next = a.Section.NextFrom;
                    }

                    var audios = a.Audios;

                    if (audios.Count == 0)
                    {
                        countTracks = listAudio.Count;
                        itsAll = true;
                    }
                    foreach (var item in audios)
                    {
                        ExtendedAudio extendedAudio = new ExtendedAudio(item, this);
                        ManualResetEvent resetEvent = new ManualResetEvent(false);

                        try
                        {
                            bool isEnqueued = DispatcherQueue.TryEnqueue(() =>
                            {
                                listAudio.Add(extendedAudio);
                                resetEvent.Set();
                            });

                            if (!isEnqueued)
                            {
                                // Действия при неудачной попытке добавления в очередь
                                Console.WriteLine("TryEnqueue не удалось добавить задачу в очередь.");
                            }

                        }
                        catch
                        {
                            throw;
                        }


                        resetEvent.WaitOne();
                    }
                    this.countTracks = this.listAudioTrue.Count;
                    NotifyOnListUpdate();
                }
                catch
                {
                    NotifyOnListUpdate();
                }
            }).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        getLoadedTracks = false;

                        Console.WriteLine(t.Exception.Message);
                    }
                    else
                    {
                        getLoadedTracks = false;
                    }
                });


        }
    }
}
