using Microsoft.UI.Dispatching;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using MusicX.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public override void GetTracks()
        {
            if (getLoadedTracks) return;
            if (itsAll) return;
            getLoadedTracks = true;

            ResponseData a = null;

                Task.Run(async () =>
                {
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
