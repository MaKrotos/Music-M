using JetBrains.Annotations;
using Newtonsoft.Json;

/* Необъединенное слияние из проекта "VkNet (netstandard2.0)"
До:
using VkNet.Utils;
После:
using System;
using VkNet.Utils;
*/

/* Необъединенное слияние из проекта "VkNet (net6.0)"
До:
using VkNet.Utils;
После:
using System;
using VkNet.Utils;
*/
using System;

namespace VkNet.Model
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class UpdateOfficeUsersResult
    {
        /// <summary>
        /// Идентификатор пользователя, добавляемого как администратор/наблюдатель.
        /// </summary>
        [JsonProperty("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// Был ли пользователь успешно изменен.
        /// </summary>
        [JsonProperty("is_success")]
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Если IsSuccess=false также возвращается объект error с описанием ошибки.
        /// </summary>
        [JsonProperty("error")]
        [CanBeNull]
        public VkError Error { get; set; }
    }
}