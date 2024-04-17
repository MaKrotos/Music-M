/* Необъединенное слияние из проекта "VkNet (netstandard2.0)"
До:
using Newtonsoft.Json;
После:
using System;
*/

/* Необъединенное слияние из проекта "VkNet (net6.0)"
До:
using Newtonsoft.Json;
После:
using System;
*/

namespace VkNet.Model.RequestParams
{
    /// <summary>
    /// Параметры для метода SavePhoto
    /// </summary>
    [Serializable]
    public class SavePhotoParams
    {
        /// <summary>
        /// Строка полученная в результате загрузки фотографии.
        /// </summary>
        public string Photo { get; set; }

        /// <summary>
        /// Хеш полученный в результате загрузки фотографии.
        /// </summary>
        public string Hash { get; set; }
    }
}