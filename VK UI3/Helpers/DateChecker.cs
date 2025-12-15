using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VK_UI3.Helpers
{
    public class DateChecker
    {
        private static readonly HttpClient client = new HttpClient();
        
        public static async Task<bool> IsSpecialDateAsync()
        {
            try
            {
                // Получаем текущую дату с бесплатного API
                var response = await client.GetStringAsync("https://smartapp-code.sberdevices.ru/tools/api/now?tz=Europe/Moscow");
                var timeData = Newtonsoft.Json.JsonConvert.DeserializeObject<SberTimeApiResult>(response);
                
                var currentDate = new DateTime(timeData.year, timeData.month, timeData.day);
                
                // Проверяем, является ли дата одной из специальных
                return currentDate.Month == 12 && currentDate.Day == 20 ||  // 20 декабря
                       currentDate.Month == 12 && currentDate.Day == 31 ||  // 31 декабря
                       currentDate.Month == 1 && currentDate.Day == 1;   // 1 января
            }
            catch
            {
                // В случае ошибки API возвращаем false
                return false;
            }
        }
        
        private class SberTimeApiResult
        {
            public int day { get; set; }
            public int month { get; set; }
            public int year { get; set; }
        }
    }
}