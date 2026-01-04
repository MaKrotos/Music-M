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
    /// <summary>
    /// Класс для работы с рекомендациями аудиозаписей
    /// </summary>
    public class Recommendations : IVKGetAudio
    {
        #region Поля и свойства
        
        /// <summary>
        /// Целевая аудиозапись
        /// </summary>
        private string targetAudio;
        
        /// <summary>
        /// Пользователь
        /// </summary>
        public User user;
        
        /// <summary>
        /// Семафор для ограничения количества одновременных запросов
        /// </summary>
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        
        #endregion
        
        #region Конструкторы
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="User_id">Идентификатор пользователя</param>
        /// <param name="dispatcher">Диспетчер</param>
        public Recommendations(long User_id, DispatcherQueue dispatcher) : base(User_id, dispatcher)
        {
            base.id = User_id.ToString();
        }
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="targetAudio">Целевая аудиозапись</param>
        /// <param name="dispatcher">Диспетчер</param>
        public Recommendations(string targetAudio, DispatcherQueue dispatcher) :base (dispatcher)
        {
           this.targetAudio = targetAudio;
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
            return null;
        }
        
        /// <summary>
        /// Получает список URI фотографий
        /// </summary>
        /// <returns>Список URI фотографий</returns>
        public override List<string> getPhotosList()
        {
            if (photoUri == null)
                return new List<string>();
            return new List<string>() { photoUri.ToString() };
        }
        
        /// <summary>
        /// Получает URI фотографии
        /// </summary>
        /// <returns>URI фотографии</returns>
        public override Uri getPhoto()
        {
            return null;
        }
        
        #endregion
        
        #region Методы для получения треков
        
        /// <summary>
        /// Получает треки
        /// </summary>
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
                catch (Exception e)
                {
                    // Обработка ошибки при получении рекомендаций
                    getLoadedTracks = false;
                    Console.WriteLine($"Ошибка при получении рекомендаций: {e.Message}");
                    // Можно также вызвать событие ошибки, если это предусмотрено
                    // onErrorLoad?.Invoke(this, new ErrorLoad(e));
                }
                finally
                {
                    semaphore.Release();
                }
                
                task = null;
            });
        }
        
        #endregion
    }
}
