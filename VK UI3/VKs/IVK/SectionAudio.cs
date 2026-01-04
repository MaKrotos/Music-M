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
    /// <summary>
    /// Класс для работы с аудиозаписями из разделов
    /// </summary>
    public class SectionAudio : IVKGetAudio
    {
        #region Поля и свойства
        
        /// <summary>
        /// Идентификатор раздела
        /// </summary>
        private string SectionID;
        
        /// <summary>
        /// Объект для синхронизации потоков
        /// </summary>
        private readonly object _lock = new object();
        
        /// <summary>
        /// Семафор для ограничения количества одновременных запросов
        /// </summary>
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        
        #endregion
        
        #region Конструкторы
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="block">Блок</param>
        /// <param name="dispatcher">Диспетчер</param>
        public SectionAudio(Block block, DispatcherQueue dispatcher) : base(block, dispatcher)
        {
        }
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="sectionID">Идентификатор раздела</param>
        /// <param name="dispatcher">Диспетчер</param>
        /// <param name="photoLink">Ссылка на фото</param>
        /// <param name="name">Название</param>
        /// <param name="audios">Список аудиозаписей</param>
        /// <param name="next">Следующий идентификатор</param>
        public SectionAudio(string sectionID, DispatcherQueue dispatcher, Uri photoLink = null, string name = null, List<Audio> audios = null, string next = null) : base(sectionID, dispatcher, photoLink, name, audios, next)
        {
        }
        
        #endregion
        
        #region Методы для получения данных
        
        /// <summary>
        /// Получает количество треков
        /// </summary>
        /// <returns>Количество треков</returns>
        public override long? getCount()
        {
            return null;
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
        /// Получает URI фотографии
        /// </summary>
        /// <returns>URI фотографии</returns>
        public override Uri getPhoto()
        {
            return null;
        }
        
        /// <summary>
        /// Получает список URI фотографий
        /// </summary>
        /// <returns>Список URI фотографий</returns>
        public override List<string> getPhotosList()
        {
            return new List<string>();
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
                            catch (Exception ex)
                            {
                                // Логируем ошибку и продолжаем выполнение
                                Console.WriteLine($"Ошибка при добавлении аудиозаписи в список: {ex.Message}");
                                throw;
                            }
                            
                            resetEvent.WaitOne();
                        }
                        this.countTracks = this.listAudio.Count;
                        NotifyOnListUpdate();
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку
                        Console.WriteLine($"Ошибка при получении аудиозаписей из раздела: {ex.Message}");
                        // Можно также вызвать событие ошибки, если это предусмотрено
                        // onErrorLoad?.Invoke(this, new ErrorLoad(ex));
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
            finally
            {
                semaphore.Release(); // Освобождает семафор
            }
        }
        
        #endregion
    }
}
