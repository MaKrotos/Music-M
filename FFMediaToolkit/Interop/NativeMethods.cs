namespace FFMediaToolkit.Interop
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Contains the native operating system methods.
    /// </summary>
    internal static class NativeMethods
    {
        private static string MacOSDefautDirectory => "/opt/local/lib/";

        private static string LinuxDefaultDirectory => $"/usr/lib/{(Environment.Is64BitOperatingSystem ? "x86_64" : "x86")}-linux-gnu";

        private static string WindowsDefaultDirectory => $@"\runtimes\{(Environment.Is64BitProcess ? "win-x64" : "win-x86")}\native";

        internal static string GetFFmpegDirectory()
        {
            // Получаем путь к исполняемому файлу
            string appFolder = AppContext.BaseDirectory;
            string ffmpegFolder = Path.Combine(appFolder, "FFmpeg");

            // Создаем папку, если она не существует
            if (!Directory.Exists(ffmpegFolder))
            {
                Directory.CreateDirectory(ffmpegFolder);
            }

            return ffmpegFolder;
        }

        /// <summary>
        /// Gets the default FFmpeg directory for current platform.
        /// </summary>
        /// <returns>A path to the default directory for FFmpeg libraries.</returns>

    }
}
