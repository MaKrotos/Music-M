using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Views.Tasks;
using VkNet.Abstractions;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using IAuthCategory = VkNet.AudioBypassService.Abstractions.Categories.IAuthCategory;

namespace VK_UI3.VKs
{
    internal class VK
    {
        public static readonly IVkApi api = App._host.Services.GetRequiredService<IVkApi>();

        public  readonly IVkApiAuthAsync _vkApi = App._host.Services.GetRequiredService<IVkApiAuthAsync>();
        public static readonly VkService vkService = App._host.Services.GetRequiredService<VkService>();


        public WeakEventManager LoggedIn = new WeakEventManager();



        public TokenChecker checker = App._host.Services.GetRequiredService<TokenChecker>();

        public static readonly BoomService boomService = App._host.Services.GetRequiredService<BoomService>();


        public static readonly SafeBoomService safeBoomService = new SafeBoomService(boomService);


        public class SafeBoomService
        {
            private readonly BoomService _boomService;

            public SafeBoomService(BoomService boomService)
            {
                _boomService = boomService;

            }

            public T CallWithRetry<T>(Func<BoomService, T> func)
            {
                int retryCount = 0;
                while (retryCount < 5)
                {
                    try
                    {
                        return func(_boomService);
                    }
                    catch (Exception ex)
                    {

                        _ = AuthBoomAsync();
                        Console.WriteLine($"Произошла ошибка: {ex.Message}");
                        retryCount++;
                    }
                }
                throw new Exception("Превышено максимальное количество попыток");
            }

            public async Task<T> CallWithRetryAsync<T>(Func<BoomService, Task<T>> func)
            {
                int retryCount = 0;
                while (retryCount < 5)
                {
                    try
                    {
                        return await func(_boomService);
                    }
                    catch (Exception ex)
                    {
                        // Выполните здесь определенное действие
                        // Например, запись в лог
                        await AuthBoomAsync();
                        Console.WriteLine($"Произошла ошибка: {ex.Message}");
                        retryCount++;
                    }
                }
                throw new Exception("Превышено максимальное количество попыток");
            }


        }


        protected static async Task AuthBoomAsync()
        {
            try
            {
                var boomVkToken = await vkService.GetBoomToken();

                var boomToken = await boomService.AuthByTokenAsync(boomVkToken.Token, boomVkToken.Uuid);

                boomService.SetToken(boomToken.AccessToken);


            }
            catch (Exception)
            {




            }


        }

        public VK()
        {


        }

     







        public static async Task BoomUpdateToken()
        {
            try
            {
                var boomVkToken = await vkService.GetBoomToken();

                var boomToken = await boomService.AuthByTokenAsync(boomVkToken.Token, boomVkToken.Uuid);

                DB.AccountsDB.activeAccount.BoomToken = boomToken.AccessToken;
                DB.AccountsDB.activeAccount.Update();

                boomService.SetToken(boomToken.AccessToken);
            }
            catch (Exception)
            {


            }
        }


        /*

        public async Task<string> GetAlbumCover(long? ownerId, long albumId)
        {

            // Убедитесь, что api не является null
            var services = new ServiceCollection();
            services.AddVkNetWithAuth();
            //    services.AddAudioBypass();

            var api = new VkApi(services);

            var actAcc = GetActiveAccounts()[0];

            api.Authorize(new ApiAuthParams
            {
                AccessToken = actAcc.Token
            });

            var audio = api.Audio.Get(new AudioGetParams
            {
                OwnerId = ownerId,
                AlbumId = albumId
            });

            var track = audio.FirstOrDefault();

            if (track != null && track.Album != null)
            {
                return track.Album.Thumb.Photo300;
            }

            return null;
        }

        */
        public IVkApi getVKAPI()
        {

            return api;
        }

        public List<VkNet.Model.Attachments.Audio> GetUserMusic()
        {
            try
            {
                var audios =
                    (api.Audio.GetAsync(new AudioGetParams
                    {
                        OwnerId = AccountsDB.GetActiveAccount().id
                    }).Result);
                return audios.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public static async Task<bool> AddDislike(long Id, long OwnerId)
        {
            try
            {
                var parameters = new VkParameters
                {
                    { "audio_ids", $"{OwnerId}_{Id}" }
                };

                var response = await api.CallAsync("audio.addDislike", parameters);

                if (response.RawJson == "1")
                {
                    return true;
                }
                else
                {


                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> RemoveDislike(long Id, long OwnerId)
        {
            try
            {
                var parameters = new VkParameters
                {
                  { "audio_ids", $"{OwnerId}_{Id}" }
                };

                var response = await api.CallAsync("audio.removeDislike", parameters);

                if (response.RawJson == "1")
                {
                    return true;
                }
                else
                {
                    // Обработка ошибок

                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> deleteFromPlaylist(long Id, long OwnerId, long playlistID)
        {
            try
            {
                var parameters = new VkParameters
                {
                    { "owner_id", OwnerId },
                    { "audio_ids", $"{OwnerId}_{Id}"  },
                    { "playlist_id", playlistID
                    }
                };


                var response = await api.CallAsync("audio.removeFromPlaylist", parameters);

                if (response.RawJson == "1")
                {
                    return true;
                }
                else
                {
                    // Обработка ошибок

                    return false;
                }
            }
            catch { return false; }
        }


        public static async Task sendStartEvent(long Id, long OwnerId, long? playlistID = null)
        {
            try
            {
                var parameters = new VkParameters
                {
                    { "uuid", Guid.NewGuid() },
                    { "audio_id", $"{OwnerId}_{Id}"  },

                };
                if (playlistID != null)
                    parameters.Add("playlist_id", playlistID);

                var response = await api.CallAsync("audio.sendStartEvent", parameters);


            }
            catch (Exception)
            {

            }
        }


        internal static async Task<string> getUploadServerAsync()
        {

            var parameters = new VkParameters
            {


            };

            try
            {

                var response = await api.CallAsync("audio.getUploadServer", parameters);
                var js = JsonObject.Parse(response.RawJson);
                return js["upload_url"].ToString();
                Console.WriteLine(response.ToString());
            }
            catch
            (Exception)
            {

            }

            return null;

        }
        internal static async Task ReorderAudio(int audio_id, int? owner_id = null, int? playlist_id = null, int? before = null, int? after = null)
        {
            var parameters = new VkParameters
            {


            };

            if (audio_id != null)
                parameters.Add("audio_id", audio_id);
            if (owner_id != null)
                parameters.Add("owner_id", owner_id);
            if (playlist_id != null)
                parameters.Add("playlist_id", playlist_id);
            if (before != null)
                parameters.Add("before", before);
            if (after != null)
                parameters.Add("after", after);

            try
            {
                var response = await api.CallAsync("audio.reorder", parameters);
            }
            catch (Exception)
            {

            }

        }



        internal static async Task deleteAllMusicFromProfile()
        {


            try
            {



                var offset = 0;

                VkCollection<VkNet.Model.Attachments.Audio> audio;
                List<VkNet.Model.Attachments.Audio> audios = new();
                do
                {
                    AudioGetParams param = new AudioGetParams()
                    {
                        OwnerId = AccountsDB.activeAccount.id,
                        Offset = offset,
                        Count = 6000

                    };
                    offset += 6000;
                    audio = await api.Audio.GetAsync(param);

                    audios.AddRange(audio);



                } while (audio.Count > 0);



                var tasks = new List<Func<Task>>();

                foreach (var item in audios)
                {
                    tasks.Add(
                        async () =>
                        {
                            _ = await api.Audio.DeleteAsync(item.Id, item.OwnerId);
                        }
                    );
                }

                _ = new TaskListActions(tasks, tasks.Count, "Удаляю треки с профиля...", null, null, 1000);
            }
            catch
            (Exception)
            {

            }


        }
    }




}

