using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.VKs;
using VkNet.Model.Attachments;
using Windows.System;

namespace VK_UI3.TempPlayLists
{
    internal class TempPlayLists
    {
        private static List<AudioPlaylist> _audioPlaylists = new List<AudioPlaylist>();
        private static DateTime lastUpdate = DateTime.MinValue;

        private const int MaxCount = 5;
        private static bool isAllLoaded = false;
        private static uint offset = 0;
        private const uint RequestOffset = 100;

        public static async Task<List<AudioPlaylist>> GetPlayListAsync(bool update = false)
        {
            if (update || (DateTime.Now - lastUpdate).TotalSeconds > 30 || (_audioPlaylists.Count() < MaxCount && !isAllLoaded))
            {
                offset = 0;
                isAllLoaded = false;
                _audioPlaylists.Clear();
                await LoadMorePlaylistsAsync();
                lastUpdate = DateTime.Now;
            }
            return _audioPlaylists;
        }

        private static async Task LoadMorePlaylistsAsync()
        {
            if (_audioPlaylists.Count >= MaxCount || isAllLoaded) return;

            var playlists = await VK.api.Audio.GetPlaylistsAsync(AccountsDB.activeAccount.id, RequestOffset, offset);

            offset += RequestOffset;

            foreach (var playlist in playlists)
            {
                if (!playlist.Permissions.Edit) continue;

                _audioPlaylists.Add(playlist);
                if (_audioPlaylists.Count >= MaxCount) break;
            }

            if (playlists.Count < RequestOffset)
            {
                isAllLoaded = true;
            }

            if (!isAllLoaded)
            {
                await LoadMorePlaylistsAsync();
            }
        }
    }
}

