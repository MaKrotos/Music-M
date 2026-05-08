using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;

namespace VK_UI3.Helpers
{
    public static class CacheManager
    {
        // HttpClient для скачивания обычных mp3 файлов
        private static readonly HttpClient _httpClient = new HttpClient();
        
        // Папка кэша внутри локальной директории данных приложения
        public static string CacheDirectory => Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "TracksCache");

        /// <summary>
        /// Проверяет, сохранен ли уже этот трек в кэше
        /// </summary>
        public static bool IsTrackCached(string trackId)
        {
            return GetCachedTrackPath(trackId) != null;
        }

        /// <summary>
        /// Возвращает локальный путь к файлу трека, если он есть, иначе null
        /// </summary>
        public static string GetCachedTrackPath(string trackId)
        {
            var mp3Path = Path.Combine(CacheDirectory, $"{trackId}.mp3");
            var m4aPath = Path.Combine(CacheDirectory, $"{trackId}.m4a");

            if (File.Exists(mp3Path)) return mp3Path;
            if (File.Exists(m4aPath)) return m4aPath;

            return null;
        }

        /// <summary>
        /// Асинхронно кэширует трек по его URL
        /// </summary>
        public static async Task<bool> CacheTrackAsync(string trackId, string url)
        {
            if (string.IsNullOrEmpty(url)) return false;

            try
            {
                if (!Directory.Exists(CacheDirectory))
                {
                    Directory.CreateDirectory(CacheDirectory);
                }

                // Если трек уже закэширован, не скачиваем его заново
                if (IsTrackCached(trackId)) return true;

                if (url.Contains(".m3u8"))
                {
                    // HLS стрим кэшируем через FFmpeg в m4a
                    var m4aPath = Path.Combine(CacheDirectory, $"{trackId}.m4a");
                    return await HlsCacher.CacheM3u8Async(url, m4aPath);
                }
                else
                {
                    // Обычный трек (mp3) скачиваем напрямую
                    var mp3Path = Path.Combine(CacheDirectory, $"{trackId}.mp3");
                    using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    using var fs = new FileStream(mp3Path, FileMode.Create, FileAccess.Write, FileShare.None);
                    await response.Content.CopyToAsync(fs);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка кэширования трека {trackId}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Удаляет все закэшированные треки
        /// </summary>
        public static void ClearCache()
        {
            if (Directory.Exists(CacheDirectory))
            {
                Directory.Delete(CacheDirectory, true);
            }
        }
    }
}