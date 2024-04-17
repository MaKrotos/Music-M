using Newtonsoft.Json;
using System;

namespace VkNet.Model
{
    /// <summary>
    /// Объект, содержащий поле count
    /// </summary>
    [Serializable]
    public class ObjectCount
    {
        /// <summary>
        /// количество *** к странице внутри виджета
        /// </summary>
        [JsonProperty(propertyName: "count")]
        public long Count { get; set; }
    }
}