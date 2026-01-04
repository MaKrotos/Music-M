using Microsoft.UI.Dispatching;
using MusicX.Core.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VkNet.Model.Attachments;
using VkNet.Utils;

namespace VK_UI3.VKs.IVK
{
    public record MixOptions(string Id, int Append = 0, ImmutableDictionary<string, ImmutableArray<string>>? Options = null)
    {
        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(Id);
            hashCode.Add(Append);
            if (Options is not null)
                foreach (var (key, values) in Options)
                {
                    hashCode.Add(key);
                    foreach (var item in values)
                    {
                        hashCode.Add(item);
                    }
                }

            return hashCode.ToHashCode();
        }
    }

    public class MixAudio : IVKGetAudio
    {
        public MixOptions data;
        public MixAudio(MixOptions data, DispatcherQueue dispatcher) : base(dispatcher)
        {
            this.data = data;

            this.countTracks = -1;
       
            Task.Run(async () =>
            {
                try
                {
                    var audios = await VK.vkService.GetStreamMixAudios(data.Id, data.Append, options: data.Options);
                 
                    foreach (var item in audios)
                    {
                        listAudio.Add(new Helpers.ExtendedAudio(item, this));
                    }
                }
                catch (Exception e)
                {
                }
                this.currentTrack = 0;
                AudioPlayer.PlayList(this);
            });
        }

        public override long? getCount()
        {
            return listAudio.Count();
        }

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

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public override void GetTracks()
        {
            semaphore.Wait(); // Ожидает освобождения семафора

            try
            {
                if (getLoadedTracks) return;
                getLoadedTracks = true;

                task = Task.Run(async () =>
                {
                    try
                    {
                        var tracks = await VK.vkService.GetStreamMixAudios(data.Id, data.Append+1, options: data.Options);
                        foreach (var item in tracks)
                        {
                            listAudio.Add(new Helpers.ExtendedAudio(item, this));
                        }
                    }
                    catch (Exception e)
                    {
                    }
                    getLoadedTracks = false;
                    NotifyOnListUpdate();
                });
            }
            finally
            {
                semaphore.Release(); 
            }
        }
    }
}
