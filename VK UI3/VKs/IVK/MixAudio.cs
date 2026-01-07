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
    /// <summary>
    /// Опции для микса
    /// </summary>
    public record MixOptions(string Id, int Append = 0, ImmutableDictionary<string, ImmutableArray<string>>? Options = null)
    {
        /// <summary>
        /// Получает хэш-код
        /// </summary>
        /// <returns>Хэш-код</returns>
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
    
    /// <summary>
    /// Класс для работы с миксами аудиозаписей
    /// </summary>
    public class MixAudio : IVKGetAudio
    {
        #region Поля и свойства
        
        /// <summary>
        /// Данные микса
        /// </summary>
        public MixOptions data;
        
        /// <summary>
        /// Семафор для ограничения количества одновременных запросов
        /// </summary>
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        
        #endregion
        
        #region Конструкторы
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="data">Данные микса</param>
        /// <param name="dispatcher">Диспетчер</param>
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
                    // Логируем ошибку
                    Console.WriteLine($"Ошибка при получении микса аудиозаписей: {e.Message}");
                    // Можно также вызвать событие ошибки, если это предусмотрено
                    // onErrorLoad?.Invoke(this, new ErrorLoad(e));
                }
                this.currentTrack = 0;
                VK_UI3.Services.MediaPlayerService.PlayList(this);
            });
        }
        
        #endregion
        
        #region Методы для получения данных
        
        /// <summary>
        /// Получает количество треков
        /// </summary>
        /// <returns>Количество треков</returns>
        public override long? getCount()
        {
            return listAudio.Count();
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
                        var tracks = await VK.vkService.GetStreamMixAudios(data.Id, data.Append+1, options: data.Options);
                        foreach (var item in tracks)
                        {
                            listAudio.Add(new Helpers.ExtendedAudio(item, this));
                        }
                    }
                    catch (Exception e)
                    {
                        // Логируем ошибку
                        Console.WriteLine($"Ошибка при получении треков микса: {e.Message}");
                        // Можно также вызвать событие ошибки, если это предусмотрено
                        // onErrorLoad?.Invoke(this, new ErrorLoad(e));
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
        
        #endregion
    }
}
