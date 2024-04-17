using MusicX.Core.Exceptions.Boom;
using MusicX.Core.Models.Boom;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Services
{
    public class BoomService : IDisposable
    {
        private readonly string deviceId = "c3427adfd2595c73:A092cf601fef615c8b594f6ad2c63d159";
        private readonly Logger logger;
        private bool isAuth = false;
        private string token = string.Empty;

        public HttpClient Client { get; }

        public BoomService(Logger logger)
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("User-Agent", "okhttp/5.0.0-alpha.10");
            Client.DefaultRequestHeaders.Add("X-App-Id", "6767438");
            Client.DefaultRequestHeaders.Add("X-Client-Version", "10265");
            Client.DefaultRequestHeaders.Add("X-Client-Version", "10265");
            Client.BaseAddress = new Uri("https://api.moosic.io/");

            var log = LogManager.Setup().GetLogger("Common");

            if (logger == null) this.logger = log;
            else this.logger = logger;
        }

        public async Task<AuthToken> AuthByTokenAsync(string slientToken, string uuid)
        {
            try
            {
                var parameters = new Dictionary<string, string>()
                {
                    {"device_id", deviceId },
                    { "device_os", "android"},
                    { "silent_token", slientToken},
                    {"uuid", uuid }
                };

                var result = await RequestAsync("oauth/vkconnect/vk/token", parameters);

                var model = JsonConvert.DeserializeObject<AuthToken>(result);

                return model;
            }catch(Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
           
        }

        public void SetToken(string token)
        {
            isAuth = true;
            this.token = token;
        }

        public async Task<User> GetUserInfoAsync()
        {
            try
            {
                var result = await RequestAsync("user/info");

                var model = JsonConvert.DeserializeObject<Response>(result);

                return model.Data.User;
            }catch(Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
           
        }

        public async Task<List<Artist>> GetArtistsAsync()
        {
            try
            {
                var result = await RequestAsync("radio/artist/profile/");

                var model = JsonConvert.DeserializeObject<Response>(result);

                return model.Data.Artists;
            }
            catch (Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<List<Tag>> GetTagsAsync()
        {
            try
            {
                var result = await RequestAsync("radio/tag/profile/");

                var model = JsonConvert.DeserializeObject<Response>(result);


                return model.Data.Tags;
            }
            catch (Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<Radio> GetPersonalMixAsync()
        {
            try
            {
                var result = await RequestAsync("radio/personal/");

                var model = JsonConvert.DeserializeObject<Response>(result);

                return model.Data.Radio;
            }
            catch (Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<Radio> GetArtistMixAsync(string artistId)
        {
            try
            {
                var result = await RequestAsync($"radio/artist/{artistId}/");

                var model = JsonConvert.DeserializeObject<Response>(result);

                 return model.Data.Radio;
            }
            catch (Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<Radio> GetTagMixAsync(string artistId)
        {
            try
            {
                var result = await RequestAsync($"radio/tag/{artistId}/");

                var model = JsonConvert.DeserializeObject<Response>(result);

                return model.Data.Radio;
            }
            catch (Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<List<Track>> GetUserTopTracks()
        {
            try
            {
                var result = await RequestAsync("user/top/tracks/");

                var model = JsonConvert.DeserializeObject<Response>(result);

                return model.Data.Tracks;
            }
            catch (Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        [Obsolete("ВК тут ничего не возвращает, твари.")]
        public async Task GetUserTopPlaylists()
        {
            try
            {
                var result = await RequestAsync("user/top/playlists/");

                var model = JsonConvert.DeserializeObject<Response>(result);

                //return new List<Track>();
            }
            catch (Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<List<Artist>> GetUserTopArtists()
        {
            try
            {
                var result = await RequestAsync("user/top/artists/");

                var model = JsonConvert.DeserializeObject<Response>(result);

                return model.Data.Artists;
            }
            catch (Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }
        
        public async Task Like(string trackApiId)
        {
            try
            {
                await RequestAsync($"track/{trackApiId}/like", httpMethod: HttpMethod.Put);
            }
            catch (Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }
        
        public async Task UnLike(string trackApiId)
        {
            try
            {
                await RequestAsync($"track/{trackApiId}/like", httpMethod: HttpMethod.Delete);
            }
            catch (Exception ex)
            {
                logger.Error("BOOM API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        private async Task<string> RequestAsync(string method, Dictionary<string, string>? arguments = null, HttpMethod? httpMethod = null)
        {
            if(isAuth)
            {
                Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var parameters = string.Empty;

            if(arguments != null)
            {
                foreach (var key in arguments)
                {
                    parameters += $"{key.Key}={key.Value}&";
                }
            } 

            using var response = await Client.SendAsync(new(httpMethod ?? HttpMethod.Get, method + "?" + parameters));

            if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
