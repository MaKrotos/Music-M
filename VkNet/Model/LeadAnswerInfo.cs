using Newtonsoft.Json;
using System;

namespace VkNet.Model
{
    /// <summary>
    /// информация об ответах на вопросы — массив структур со следующими полями
    /// </summary>
    [Serializable]
    public class LeadAnswerInfo
    {
        /// <summary>
        /// ключ вопроса.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// ответ на вопрос
        /// </summary>
        [JsonProperty("answer")]
        public LeadAnswer Answer { get; set; }
    }
}