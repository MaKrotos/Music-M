using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TagLib.Ape;
using VK_UI3.Helpers;
using VK_UI3.Views.Share;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VK_UI3.VKs.IVK
{
    public class MessagesAudio : IVKGetAudio
    {
        public MessagesAudio(MessConv messConv, DispatcherQueue dispatcher) : base(dispatcher)
        {
            base.id = messConv.conversation.Peer.LocalId.ToString();
            this.messConv = messConv;
        }
       
        
        public override long? getCount()
        {
            return -1;
        }

        MessConv messConv = null;
        public override string getName()
        {
            try
            {
                switch (messConv.conversation.Peer.Type.ToString())
                {
                    case "chat":
                        return messConv.conversation.ChatSettings.Title;
                        

                        break;
                    case "user":
                        return messConv.user.FirstName +" "+messConv.user.LastName;
                        break;
                    case "group":
                        return messConv.group.Name;
                        break;
                    case "email":

                        break;
                    default:
                        break;
                }

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
            switch (messConv.conversation.Peer.Type.ToString())
            {
                case "chat":
                    return messConv.conversation.ChatSettings.Photo.JustGetPhoto;

                    break;
                case "user":
                    return messConv.user.JustGetPhoto;
                    break;
                case "group":
                    return messConv.group.JustGetPhoto;
                    break;
                case "email":

                    break;
                default:
                    break;
            }
            return null;
        }
        string nextFrom = null;
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
                    int count = 100;
                    VkCollection<Audio> audios;

                    MessagesGetHistoryAttachmentsParams messagesGetHistoryAttachmentsParams = new MessagesGetHistoryAttachmentsParams()
                    {
                        Count = count,
                        MediaType = MediaType.Audio,
                        PeerId = messConv.conversation.Peer.Id,
                        StartFrom = nextFrom
                    };

                    var attach = (VK.api.Messages.GetHistoryAttachments(messagesGetHistoryAttachmentsParams, out nextFrom));
                    ManualResetEvent resetEvent = new ManualResetEvent(false);
                    foreach (var item in attach)
                    {
                        ExtendedAudio extendedAudio = new ExtendedAudio(item.Attachment.Instance as Audio, this);

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            listAudio.Add(extendedAudio);
                            resetEvent.Set();
                        });

                        resetEvent.WaitOne(); 
                        resetEvent.Reset(); 
                    }

                    

                    if (nextFrom == null) itsAll = true;

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
