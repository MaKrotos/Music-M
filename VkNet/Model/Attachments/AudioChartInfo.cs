using Newtonsoft.Json;

namespace VkNet.Model.Attachments
{
    /// <summary>
    /// Инфо о чарте трека
    /// </summary>
    public class AudioChartInfo
    {
        [JsonProperty("position")]
        /// <summary>
        /// Позиция
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Состояние
        /// </summary>

        [JsonProperty("state")]
        public ChartState State { get; set; }

    
    }
    /// <summary>
    /// Состояние чарта
    /// </summary>
    public enum ChartState
    {
        /// <summary>
        /// Новый релиз
        /// </summary>
        New_Release = 0,
        /// <summary>
        /// Не изменялся
        /// </summary>
        No_Changes = 1,
        /// <summary>
        /// Выше
        /// </summary>
        Moved_up = 2,
        /// <summary>
        /// Ниже
        /// </summary>
        Moved_down = 3,

        /// <summary>
        /// Первый
        /// </summary>
        Crown = 4
    }
}