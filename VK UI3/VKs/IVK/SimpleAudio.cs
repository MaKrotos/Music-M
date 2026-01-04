using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model;

namespace VK_UI3.VKs.IVK
{
    /// <summary>
    /// Простая реализация IVKGetAudio для работы с аудиозаписями
    /// </summary>
    public class SimpleAudio : IVKGetAudio
    {
        #region Конструкторы
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="dispatcher">Диспетчер</param>
        public SimpleAudio(DispatcherQueue dispatcher) : base(dispatcher)
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
            return listAudio.Count();
        }
        
        /// <summary>
        /// Пользователь
        /// </summary>
        private User user;
        
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
            itsAll = true;
            return;
        }
        
        #endregion
    }
}
