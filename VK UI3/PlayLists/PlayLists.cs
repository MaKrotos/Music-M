using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using VK_UI3.VKs;
using VkNet.Model.Attachments;
using VkNet.Utils;

namespace VK_UI3.PlayLists
{
    internal class PlayLists
    {

        public static TimedDictionary<long, List<AudioPlaylist>> playLists = new();

        public static async Task<List<AudioPlaylist>> getListPlayLists(long id) {
            uint count = 100;
            uint offset = 0;
            VkCollection<AudioPlaylist> collection = null;
            List<AudioPlaylist> list = null;
            
            do
            {
                collection = await VK.api.Audio.GetPlaylistsAsync(id,  count,  offset);
                foreach (var item in list)
                {
                    list.Add(item);
                }
           

            } while (collection.Count() == count);

            playLists.Add(id, list);
            return playLists[id];
        }


    }
}
