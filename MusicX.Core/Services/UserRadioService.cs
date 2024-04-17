using MusicX.Shared.ListenTogether.Radio;
using NLog;
using System.Net.Http.Json;
using VkNet.Abstractions;

namespace MusicX.Core.Services
{
    public class UserRadioService
    {
        private readonly Logger _logger;
        private readonly ListenTogetherService _listenTogetherService;
        private readonly IUsersCategory _usersCategory;

        public bool IsStarted { get; private set; }

        public UserRadioService(Logger logger, ListenTogetherService listenTogetherService, IUsersCategory usersCategory)
        {
            _logger = logger;
            _listenTogetherService = listenTogetherService;
            _usersCategory = usersCategory;
        }

        public async Task<List<Station>> GetStationsList()
        {
            _logger.Info("Получение списка всех доступных станций");

            var stations = await HttpRequestAsync<List<Station>>("getStations", new Dictionary<string, string>());

            _logger.Info($"Получено {stations.Count} пользовательских станций");

            return stations;
        }

        public Task<Station> CreateStationAsync(string sessionId, string title, string cover, string decription, long ownerId, string ownerName, string ownerPhoto)
        {
            _logger.Info($"Создание пользовательской радиостанции с ID {sessionId}");

            var p = new Dictionary<string, string>()
            {
                {"sessionId", sessionId },
                {"title", title},
                {"cover", cover },
                {"decription", decription },
                {"ownerId", ownerId.ToString() },
                {"ownerName", ownerName },
                {"ownerPhoto", ownerPhoto}
            };

            IsStarted = true;

            return HttpRequestAsync<Station>("createStation", p);
        }

        public Task<bool> DeleteStationAsync(string sessionId)
        {
            _logger.Info($"Удаление пользовательской радиостанции с ID {sessionId}");

            var p = new Dictionary<string, string>()
            {
                {"sessionId", sessionId }
            };

            IsStarted = false;

            return HttpRequestAsync<bool>("deleteStation", p);
        }

        public async Task<string> UploadCoverAsync(string path)
        {
            using var httpClient = await GetHttpClientAsync();
            await using var stream = File.OpenRead(path);

            var data = new MultipartFormDataContent
            {
                { new StreamContent(stream), "image", "image" }
            };

            using var response = await httpClient.PostAsync("/radio/uploadImage", data);

            response.EnsureSuccessStatusCode();

            return $"{_listenTogetherService.Host}/{await response.Content.ReadAsStringAsync()}";
        }

        private async Task<TResponse> HttpRequestAsync<TResponse>(string method, Dictionary<string, string> parameters)
        {
            try
            {
                using var httpClient = await GetHttpClientAsync();

                var p = parameters.Select(x => x.Key + "=" + x.Value);

                return await httpClient.GetFromJsonAsync<TResponse>("/radio/" + method + "?" + string.Join("&", p));
            }
            catch (Exception ex)
            {
                _logger.Info($"Произошла ошибка при запросе: {ex}");
                _logger.Error(ex);
                throw;
            }

        }

        private async ValueTask<HttpClient> GetHttpClientAsync()
        {
            if (_listenTogetherService.Token is null)
            {
                var users = await _usersCategory.GetAsync(Enumerable.Empty<long>());
                await _listenTogetherService.LoginAsync(users[0].Id);
            }

            return new HttpClient
            {
                BaseAddress = new Uri(_listenTogetherService.Host),
                DefaultRequestHeaders =
                    {
                        Authorization = new("Bearer", _listenTogetherService.Token)
                    }
            };
        }
    }
}
