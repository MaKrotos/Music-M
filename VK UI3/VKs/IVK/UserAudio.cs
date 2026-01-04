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
    /// Класс для работы с аудиозаписями пользователя
    /// </summary>
    public class UserAudio : IVKGetAudio, IVKGetTracksMore
    {
        #region Поля и свойства
        
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
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="dispatcher">Диспетчер</param>
        public UserAudio(long id, DispatcherQueue dispatcher) : base(id, dispatcher)
        {
            base.id = id.ToString();
        }
        
        #endregion
        
        #region Методы для получения данных
        
        /// <summary>
        /// Получает количество треков
        /// </summary>
        /// <returns>Количество треков</returns>
        public override long? getCount()
        {
            try
            {
                return api.Audio.GetCountAsync(long.Parse(id)).Result;
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine($"Ошибка при получении количества аудиозаписей пользователя: {ex.Message}");
                // Можно также вызвать событие ошибки, если это предусмотрено
                // onErrorLoad?.Invoke(this, new ErrorLoad(ex));
                return null;
            }
        }
        
        /// <summary>
        /// Получает название
        /// </summary>
        /// <returns>Название</returns>
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
            if (user != null)
            {
                return
                    user.PhotoMaxOrig;
            }
            else
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
                                    resetEvent.Set(); // Сигнализирует о завершении задачи
                                });
                                
                                resetEvent.WaitOne(); // Ожидает сигнала о завершении задачи
                                resetEvent.Reset(); // Сбрасывает событие для следующей итерации
                            }
                            
                            if (countTracks == listAudio.Count()) itsAll = true;
                            
                            getLoadedTracks = false;
                        }
                        task = null;
                        NotifyOnListUpdate();
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку
                        Console.WriteLine($"Ошибка при получении аудиозаписей пользователя: {ex.Message}");
                        // Можно также вызвать событие ошибки, если это предусмотрено
                        // onErrorLoad?.Invoke(this, new ErrorLoad(ex));
                        getLoadedTracks = false;
                        task = null;
                        NotifyOnListUpdate();
                    }
                });
            }
            finally
            {
                semaphore.Release(); // Освобождает семафор
            }
        }
        
        /// <summary>
        /// Получает треки с указанным количеством
        /// </summary>
        /// <param name="counter">Количество треков</param>
        public void GetTracks(int counter)
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
                        int offset = listAudio.Count;
                        int count = counter;
                        
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
                                    resetEvent.Set(); // Сигнализирует о завершении задачи
                                });
                                
                                resetEvent.WaitOne(); // Ожидает сигнала о завершении задачи
                                resetEvent.Reset(); // Сбрасывает событие для следующей итерации
                            }
                            
                            if (countTracks == listAudio.Count()) itsAll = true;
                            
                            getLoadedTracks = false;
                        }
                        task = null;
                        NotifyOnListUpdate();
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку
                        Console.WriteLine($"Ошибка при получении аудиозаписей пользователя: {ex.Message}");
                        // Можно также вызвать событие ошибки, если это предусмотрено
                        // onErrorLoad?.Invoke(this, new ErrorLoad(ex));
                        getLoadedTracks = false;
                        task = null;
                        NotifyOnListUpdate();
                    }
                });
            }
            catch (Exception e)
            {
                semaphore.Release(); // Освобождает семафор
                throw e;
            }
            finally
            {
                semaphore.Release(); // Освобождает семафор
            }
        }
        
        #endregion
    }
}
