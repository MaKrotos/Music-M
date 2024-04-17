using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VkNet.Model
{
    /// <summary>
    /// Объект описывающий поступивший ответ.
    /// </summary>
    [Serializable]
    public class Feedback
    {
        /// <summary>
        /// Количество
        /// </summary>
        [JsonProperty(propertyName: "count")]
        public long Count { get; set; }

        /// <summary>
        /// Массив объектов
        /// </summary>
        [JsonProperty(propertyName: "items")]
        public List<FeedbackItem> Items { get; set; }
    }
}