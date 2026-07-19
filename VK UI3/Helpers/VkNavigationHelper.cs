using MusicX.Core.Models;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using VK_UI3.Views;
using static VK_UI3.Views.SectionView;

namespace VK_UI3.Helpers
{
    public static class VkNavigationHelper
    {
        /// <summary>
        /// Пытается открыть контент через внутреннюю навигацию приложения.
        /// Если не получается — возвращает false, чтобы вызвавший код открыл URL в браузере.
        /// </summary>
        public static bool TryNavigate(ListeningContentItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Url))
                return false;

            try
            {
                return item.Type switch
                {
                    ContentType.Playlist => TryOpenPlaylist(item.Url),
                    ContentType.Album => TryOpenAlbum(item.Url),
                    ContentType.Artist => TryOpenArtist(item.Url),
                    ContentType.Track => false, // Треки открываем только в браузере
                    _ => false
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VkNavigationHelper] Error navigating to {item.Url}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Пытается открыть плейлист по URL вида:
        /// https://vk.ru/music/playlist/{ownerId}_{playlistId}_{accessKey}
        /// https://vk.com/music/playlist/{ownerId}_{playlistId}_{accessKey}
        /// </summary>
        private static bool TryOpenPlaylist(string url)
        {
            // Формат: https://vk.ru/music/playlist/12345_67890_abc123
            if (!IsPlaylistUrl(url))
                return false;

            var parts = ExtractPlaylistParts(url);
            if (parts == null)
                return false;

            MainView.OpenPlayList(parts.Value.playlistId, parts.Value.ownerId, parts.Value.accessKey);
            return true;
        }

        /// <summary>
        /// Пытается открыть альбом по URL вида:
        /// https://vk.com/music/album/{ownerId}_{albumId}_{accessKey}
        /// </summary>
        private static bool TryOpenAlbum(string url)
        {
            // Формат: https://vk.com/music/album/12345_67890_abc123
            if (!IsAlbumUrl(url))
                return false;

            var parts = ExtractPlaylistParts(url);
            if (parts == null)
                return false;

            MainView.OpenPlayList(parts.Value.playlistId, parts.Value.ownerId, parts.Value.accessKey);
            return true;
        }

        /// <summary>
        /// Пытается открыть артиста по URL вида:
        /// https://vk.ru/artist/{artistId}
        /// https://vk.com/artist/{artistId}
        /// </summary>
        private static bool TryOpenArtist(string url)
        {
            // Формат: https://vk.ru/artist/SomeArtistName или https://vk.com/artist/SomeArtistName
            var uri = new Uri(url);
            var segments = uri.Segments;

            // segments: /, artist/, {artistId}/
            if (segments.Length < 2)
                return false;

            // Ищем сегмент "artist/" и берём следующий за ним
            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (segments[i].TrimEnd('/').Equals("artist", StringComparison.OrdinalIgnoreCase))
                {
                    var artistId = segments[i + 1].TrimEnd('/');
                    if (!string.IsNullOrWhiteSpace(artistId))
                    {
                        MainView.OpenSection(artistId, SectionType.Artist);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Извлекает ownerId, playlistId и accessKey из URL плейлиста/альбома.
        /// Ожидаемый формат: .../playlist/{ownerId}_{playlistId}_{accessKey}
        /// </summary>
        private static (long ownerId, long playlistId, string accessKey)? ExtractPlaylistParts(string url)
        {
            try
            {
                var uri = new Uri(url);
                var lastSegment = uri.Segments[^1].TrimEnd('/');
                var splited = lastSegment.Split('_');

                if (splited.Length < 2)
                    return null;

                if (!long.TryParse(splited[0], out var ownerId))
                    return null;

                if (!long.TryParse(splited[1], out var playlistId))
                    return null;

                var accessKey = splited.Length >= 3 ? splited[2] : null;

                return (ownerId, playlistId, accessKey);
            }
            catch
            {
                return null;
            }
        }

        private static bool IsPlaylistUrl(string url)
        {
            return Regex.IsMatch(url, @"https://vk\.(ru|com)/music/playlist/\d+_\d+_[a-z0-9]+$", RegexOptions.IgnoreCase);
        }

        private static bool IsAlbumUrl(string url)
        {
            return Regex.IsMatch(url, @"https://vk\.(com|ru)/music/album/\d+_\d+_[a-z0-9]+$", RegexOptions.IgnoreCase);
        }
    }
}