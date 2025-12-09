using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace VK_UI3.Helpers
{
    public class NotificationItem
    {
        public int Id { get; set; }
        public string Header { get; set; }
        public string Message { get; set; }
        public List<NotificationLink> Links { get; set; } = new List<NotificationLink>();
    }

    public class NotificationLink
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    internal class Notifications
    {
        private const string NotificationsUrl = "https://vkm.makrotos.ru/notifications.json";
        private HttpClient _client;

        public Notifications()
        {
            _client = new HttpClient();
            // Можно добавить таймаут при необходимости
            // _client.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<List<NotificationItem>> GetNotificationsAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(NotificationsUrl);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();
                return ParseNotifications(jsonString);
            }
            catch (HttpRequestException ex)
            {
                // Логирование ошибки сети
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки уведомлений: {ex.Message}");
                return new List<NotificationItem>();
            }
            catch (Exception ex)
            {
                // Логирование других ошибок
                System.Diagnostics.Debug.WriteLine($"Ошибка обработки уведомлений: {ex.Message}");
                return new List<NotificationItem>();
            }
        }

        private List<NotificationItem> ParseNotifications(string jsonString)
        {
            var notifications = new List<NotificationItem>();
            var lastIdStr = DB.SettingsTable.GetSetting("noteLastId");
            var lastID = 0;
            if (lastIdStr != null && int.TryParse(lastIdStr.settingValue, out int id))
            {
                lastID = id;
            }
               
            try
            {
                JsonArray jsonArray = JsonArray.Parse(jsonString);

                foreach (IJsonValue item in jsonArray)
                {
                    if (item.ValueType == JsonValueType.Object)
                    {
                        JsonObject jsonObject = item.GetObject();
                        var notification = new NotificationItem();

                        // Парсинг обязательных полей
                        if (jsonObject.ContainsKey("id"))
                        {
                            if (lastID >= (int)jsonObject["id"].GetNumber())
                                continue;
                            notification.Id = (int)jsonObject["id"].GetNumber();

                            DB.SettingsTable.SetSetting("noteLastId", ((int)jsonObject["id"].GetNumber()).ToString());
                        }


                        if (jsonObject.ContainsKey("Header"))
                            notification.Header = jsonObject["Header"].GetString();

                        if (jsonObject.ContainsKey("Message"))
                            notification.Message = jsonObject["Message"].GetString();

                        // Парсинг ссылок (опционально)
                        if (jsonObject.ContainsKey("links"))
                        {
                            JsonArray linksArray = jsonObject["links"].GetArray();
                            foreach (IJsonValue linkValue in linksArray)
                            {
                                if (linkValue.ValueType == JsonValueType.Object)
                                {
                                    JsonObject linkObject = linkValue.GetObject();
                                    var link = new NotificationLink();

                                    if (linkObject.ContainsKey("name"))
                                        link.Name = linkObject["name"].GetString();

                                    if (linkObject.ContainsKey("url"))
                                        link.Url = linkObject["url"].GetString();

                                    notification.Links.Add(link);
                                }
                            }
                        }

                        notifications.Add(notification);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка парсинга JSON: {ex.Message}");
            }

            return notifications;
        }

        

        // Метод для проверки наличия новых уведомлений
        public async Task<bool> HasNewNotificationsAsync(HashSet<int> knownNotificationIds)
        {
            var notifications = await GetNotificationsAsync();

            foreach (var notification in notifications)
            {
                if (!knownNotificationIds.Contains(notification.Id))
                {
                    return true;
                }
            }

            return false;
        }

        // Очистка ресурсов
        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}