using Newtonsoft.Json;
using MusicX.Core.Models;

namespace MusicX.Core.Services
{
    public class WhatListeningService : IWhatListeningService
    {
        private readonly HttpClient _httpClient;
        private const string DefaultUrl = "https://vkm.makrotos.ru/WhatListening.json"; // Replace with actual URL

        public WhatListeningService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ListeningItem>> GetWhatListeningAsync()
        {
            try
            {
                string json = await _httpClient.GetStringAsync(DefaultUrl);
                return JsonConvert.DeserializeObject<List<ListeningItem>>(json) ?? new List<ListeningItem>();
            }
            catch (Exception)
            {
                return await GetWhatListeningFromLocalFileAsync();
            }
        }

        private async Task<List<ListeningItem>> GetWhatListeningFromLocalFileAsync()
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views/WhatListeningSample.json");
                if (System.IO.File.Exists(path))
                {
                    string json = await System.IO.File.ReadAllTextAsync(path);
                    return JsonConvert.DeserializeObject<List<ListeningItem>>(json) ?? new List<ListeningItem>();
                }
            }
            catch { }
            return new List<ListeningItem>();
        }
    }
}