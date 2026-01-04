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
    /// <summary>
    /// Класс для работы с аудиозаписями из сообщений
    /// </summary>
    public class MessagesAudio : IVKGetAudio
    {
        #region Поля и свойства
        
        /// <summary>
        /// Беседа
        /// </summary>
        private MessConv messConv = null;
        
        /// <summary>
        /// Следующий идентификатор для загрузки
        /// </summary>
        private string nextFrom = null;
        
        /// <summary>
        /// Семафор для ограничения количества одновременных запросов
        /// </summary>
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        
        #endregion
        
        #region Конструкторы
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="messConv">Беседа</param>
        /// <param name="dispatcher">Диспетчер</param>
        public MessagesAudio(MessConv messConv, DispatcherQueue dispatcher) : base(dispatcher)
        {
            base.id = messConv.conversation.Peer.LocalId.ToString();
            this.messConv = messConv;
        }
        
        #endregion
        
        #region Методы для получения данных
        
        /// <summary>
        /// Получает количество треков
        /// </summary>
        /// <returns>Количество треков</returns>
        public override long? getCount()
        {
            return -1;
        }
        
        /// <summary>
        /// Получает название
        /// </summary>
        /// <returns>Название</returns>
        public override string getName()
        {
            try
            {
                switch (messConv.conversation.Peer.Type.ToString())
                {
                    case "chat":
                        return messConv.conversation.ChatSettings.Title;
                    
                    case "user":
                        return messConv.user.FirstName + " " + messConv.user.LastName;
                    case "group":
                        return messConv.group.Name;
                    case "email":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку и возвращаем null
                Console.WriteLine($"Ошибка при получении названия: {ex.Message}");
                // Можно также вызвать событие ошибки, если это предусмотрено
                // onErrorLoad?.Invoke(this, new ErrorLoad(ex));
            }
            
            return null;
        }
        
        /// <summary>
        /// Получает список URI фотографий
        /// </summary>
        /// <returns>Список URI фотографий</returns>
        public override List<string> getPhotosList()
        {
            if (photoUri == null) { return new List<string>(); }
            else return new List<string>() { photoUri.ToString() };
        }
        
        /// <summary>
        /// Получает URI фотографии
        /// </summary>
        /// <returns>URI фотографии</returns>
        public override Uri getPhoto()
        {
            try
            {
                switch (messConv.conversation.Peer.Type.ToString())
                {
                    case "chat":
                        return messConv.conversation.ChatSettings.Photo.JustGetPhoto;
                    
                    case "user":
                        return messConv.user.JustGetPhoto;
                    case "group":
                        return messConv.group.JustGetPhoto;
                    case "email":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку и возвращаем null
                Console.WriteLine($"Ошибка при получении фотографии: {ex.Message}");
            }
            return null;
        }
        
        #endregion
        
        #region Методы для получения треков
        
        /// <summary>
        /// Получает треки
        /// </summary>
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
                    }
                    catch (Exception ex)
                    {
                        // Обработка ошибки при получении треков
                        getLoadedTracks = false;
                        Console.WriteLine($"Ошибка при получении треков: {ex.Message}");
                        // Можно также вызвать событие ошибки, если это предусмотрено
                        // onErrorLoad?.Invoke(this, new ErrorLoad(ex));
                    }
                });
            }
            finally
            {
                semaphore.Release(); 
            }
        }
        
        #endregion
    }
}
