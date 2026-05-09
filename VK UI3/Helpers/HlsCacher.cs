using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace VK_UI3.Helpers
{
    public static class HlsCacher
    {
        /// <summary>
        /// Скачивает, расшифровывает и кэширует m3u8 (HLS) поток в единый локальный аудиофайл.
        /// </summary>
        /// <param name="m3u8Url">Прямая ссылка на .m3u8 манифест</param>
        /// <param name="outputFilePath">Локальный путь для сохранения (например, ...\Cache\track_123.m4a)</param>
        /// <returns>True, если трек успешно сохранен</returns>
        public static async Task<bool> CacheM3u8Async(string m3u8Url, string outputFilePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(outputFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Используем FFmpeg для загрузки, склейки и дешифровки сегментов.
                // -y : перезаписывать файл, если он существует
                // -i : входной URL
                // -c copy : копируем аудиопоток без пережатия (сохраняет оригинальное качество, работает очень быстро)
                var processInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg", // ffmpeg.exe должен находиться в папке с приложением или быть добавлен в PATH
                    Arguments = $"-y -i \"{m3u8Url}\" -c copy \"{outputFilePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process == null) return false;
                    await process.WaitForExitAsync();
                    return process.ExitCode == 0 && File.Exists(outputFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при кэшировании m3u8: {ex.Message}");
                return false;
            }
        }
    }
}