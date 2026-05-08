using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Windows.Storage;

namespace VK_UI3.Services
{
    public static class AudioCacheService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly ConcurrentDictionary<string, Task> _downloadTasks = new();
        private static readonly SemaphoreSlim _downloadSemaphore = new SemaphoreSlim(3);

        /// <summary>
        /// Возвращает локальный URI закэшированного аудиофайла (если есть).
        /// Иначе возвращает веб-ссылку для мгновенного воспроизведения и запускает кэширование в фоне.
        /// </summary>
        /// <param name="trackId">Уникальный ID трека (например, OwnerId_AudioId)</param>
        /// <param name="webUrl">Прямая ссылка на mp3</param>
        /// <returns>URI для воспроизведения (локальный или веб)</returns>
        public static Uri GetOrCacheAudio(string trackId, string webUrl)
        {
            string cacheFolderPath = ApplicationData.Current.LocalCacheFolder.Path;
            string filePath = Path.Combine(cacheFolderPath, $"{trackId}.mp3");

            if (File.Exists(filePath))
            {
                // Файл уже скачан, возвращаем локальный путь
                return new Uri(filePath);
            }

            // Запускаем фоновое скачивание, если оно еще не идет
            if (!_downloadTasks.ContainsKey(trackId))
            {
                var downloadTask = Task.Run(async () =>
                {
                    try
                    {
                        await DownloadAudioAsync(webUrl, filePath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AudioCacheService] Error caching {trackId}: {ex.Message}");
                    }
                    finally
                    {
                        _downloadTasks.TryRemove(trackId, out _);
                    }
                });

                _downloadTasks.TryAdd(trackId, downloadTask);
            }

            // Отдаем веб-ссылку для моментального старта воспроизведения
            return new Uri(webUrl);
        }

        /// <summary>
        /// Запускает фоновое кэширование списка треков.
        /// </summary>
        public static async Task CacheTracksAsync(IEnumerable<VK_UI3.Helpers.ExtendedAudio> tracks, IProgress<double> progress = null, CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>();
            int total = 0;
            int completed = 0;

            foreach (var track in tracks)
            {
                if (cancellationToken.IsCancellationRequested) break;

                if (track.audio.Url == null) continue;
                total++;

                string trackId = $"{track.audio.OwnerId}_{track.audio.Id}";
                string webUrl = track.audio.Url.ToString();
                string cacheFolderPath = ApplicationData.Current.LocalCacheFolder.Path;
                string filePath = Path.Combine(cacheFolderPath, $"{trackId}.mp3");

                if (File.Exists(filePath))
                {
                    // Файл уже скачан
                    var c = System.Threading.Interlocked.Increment(ref completed);
                    progress?.Report((double)c / total * 100);
                }
                else if (!_downloadTasks.ContainsKey(trackId))
                {
                    var downloadTask = Task.Run(async () =>
                    {
                        try { await DownloadAudioAsync(webUrl, filePath, cancellationToken); }
                        catch (OperationCanceledException) { /* Игнорируем исключение отмены (файл сам удалится в DownloadAudioAsync) */ }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[AudioCacheService] Error caching {trackId}: {ex.Message}"); }
                        finally { _downloadTasks.TryRemove(trackId, out _); }
                    }, cancellationToken);

                    _downloadTasks.TryAdd(trackId, downloadTask);
                    
                    tasks.Add(downloadTask.ContinueWith(t => 
                    {
                        var c = System.Threading.Interlocked.Increment(ref completed);
                        progress?.Report((double)c / total * 100);
                    }));
                }
                else if (_downloadTasks.TryGetValue(trackId, out var existingTask))
                {
                    tasks.Add(existingTask.ContinueWith(t => 
                    {
                        var c = System.Threading.Interlocked.Increment(ref completed);
                        progress?.Report((double)c / total * 100);
                    }));
                }
            }
            
            if (tasks.Any())
                await Task.WhenAll(tasks);
        }

        private static async Task DownloadAudioAsync(string webUrl, string filePath, CancellationToken cancellationToken = default)
        {
            await _downloadSemaphore.WaitAsync(cancellationToken); // Ограничиваем количество одновременных загрузок
            string tempFilePath = filePath + ".tmp";
            try
            {
                using var response = await _httpClient.GetAsync(webUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs, cancellationToken);
                }

                // Переименовываем после успешной загрузки (защита от битых файлов)
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempFilePath, filePath);

                // Запускаем проверку и очистку кэша в фоне
                _ = Task.Run(CheckAndCleanupCacheAsync);
            }
            catch (OperationCanceledException)
            {
                // Подчищаем временный файл, если загрузка прервана пользователем
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { }
                }
                throw; // Пробрасываем ошибку для корректной отмены Task
            }
            finally
            {
                _downloadSemaphore.Release();
            }
        }

        private static void CheckAndCleanupCacheAsync()
        {
            try
            {
                string cacheFolderPath = ApplicationData.Current.LocalCacheFolder.Path;
                var directory = new DirectoryInfo(cacheFolderPath);
                
                // Получаем все .mp3 файлы, отсортированные по времени последнего доступа (от старых к новым)
                var files = directory.GetFiles("*.mp3").OrderBy(f => f.LastAccessTime).ToList();

                long currentSize = files.Sum(f => f.Length);
                
                // Получаем лимит из настроек (по умолчанию 1024 МБ)
                var localSettings = ApplicationData.Current.LocalSettings;
                long maxSizeMb = localSettings.Values["AudioCacheMaxSize"] as int? ?? 1024;
                long maxSizeBytes = maxSizeMb * 1024 * 1024; // перевод в байты

                if (currentSize > maxSizeBytes)
                {
                    // Удаляем старые треки, пока размер не станет равен 90% от максимума
                    long targetSize = (long)(maxSizeBytes * 0.9);
                    foreach (var file in files)
                    {
                        if (currentSize <= targetSize) break;
                        
                        try
                        {
                            currentSize -= file.Length;
                            file.Delete();
                        }
                        catch { /* Файл может быть занят плеером, пропускаем */ }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioCacheService] Cleanup error: {ex.Message}");
            }
        }

        /// <summary>
        /// Подсчитывает текущий размер кэша аудиофайлов на диске в мегабайтах.
        /// </summary>
        public static async Task<double> GetCacheSizeInMbAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    string cacheFolderPath = ApplicationData.Current.LocalCacheFolder.Path;
                    var directory = new DirectoryInfo(cacheFolderPath);

                    if (directory.Exists)
                    {
                        long size = directory.GetFiles("*.mp3").Sum(f => f.Length) + 
                                    directory.GetFiles("*.tmp").Sum(f => f.Length);
                        return Math.Round(size / 1024.0 / 1024.0, 2);
                    }
                }
                catch { }
                return 0.0;
            });
        }

        /// <summary>
        /// Полностью очищает все скачанные и временные аудиофайлы с диска.
        /// </summary>
        public static async Task ClearCacheAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    string cacheFolderPath = ApplicationData.Current.LocalCacheFolder.Path;
                    var directory = new DirectoryInfo(cacheFolderPath);

                    if (directory.Exists)
                    {
                        // Берем и .mp3 и недокачанные .tmp файлы
                        var files = directory.GetFiles("*.mp3").Concat(directory.GetFiles("*.tmp"));
                        foreach (var file in files)
                        {
                            try { file.Delete(); } catch { /* Игнорируем файлы, которые сейчас проигрываются */ }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AudioCacheService] ClearCache error: {ex.Message}");
                }
            });
        }
    }
}