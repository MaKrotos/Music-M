using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using MusicX.Core.Models;
using MusicX.Shared.Extensions;
using MusicX.Shared.ListenTogether;
using MusicX.Shared.Player;
using Newtonsoft.Json;
using NLog;

namespace MusicX.Core.Services
{
    public class ListenTogetherService : IAsyncDisposable
    {
        private readonly Logger _logger;
        private HubConnection _connection;
        private IEnumerable<IDisposable> _subscriptions;

        /// <summary>
        /// Изменилась позиция трека
        /// </summary>
        public event Func<TimeSpan, bool, Task>? PlayStateChanged;

        /// <summary>
        /// Изменился трек
        /// </summary>
        public event Func<PlaylistTrack, Task>? TrackChanged;

        /// <summary>
        /// Текущий пользователь подключился как слушатель
        /// </summary>
        public event Func<PlaylistTrack, Task>? ConnectedToSession;

        /// <summary>
        /// Текущий пользователь запустил сессию
        /// </summary>
        public event Func<string, Task>? StartedSession;

        /// <summary>
        /// К сессии подключился слушатель
        /// </summary>
        public event Func<User, Task>? ListenerConnected;

        /// <summary>
        /// От сесси отключился слушатель
        /// </summary>
        public event Func<User, Task>? ListenerDisconnected;

        /// <summary>
        /// Владелец сессии закрыл её.
        /// </summary>
        public event Func<Task>? SessionOwnerStoped;

        /// <summary>
        /// Текущий пользователь закрыл сессию.
        /// </summary>
        public event Func<Task>? SessionStoped;

        /// <summary>
        /// Текущий пользователь отключился от сесиии.
        /// </summary>
        public event Func<Task>? LeaveSession;

        public bool IsConnectedToServer => _connection is not null;

        public PlayerMode PlayerMode { get; set; } = PlayerMode.None;

        public string SessionId { get; set; }

        public string ConnectUrl => $"{Host}/connect?id={SessionId}";

        public string Host { get; private set; } = "https://musicx.zznty.ru:8443";
        public string? Token { get; set; }

        public ListenTogetherService(Logger logger)
        {
            _logger = logger;

            SessionOwnerStoped += SessionUserStoped;
        }


        public async Task<string> StartSessionAsync(long userId)
        {
            _logger.Info("Запуск сессии Слушать вместе");
            if (_connection is null)
            {
                await ConnectToServerAsync(userId);
            }

            var result = await _connection.InvokeAsync<SessionId>(ListenTogetherMethods.StartPlaySession);

            if(result is null)
            {
                throw new Exception("Произошла ошибка при создании сессии");
            }

            PlayerMode = PlayerMode.Owner;

            SessionId = result.Id;
            
            _logger.Info($"Сессия {SessionId} запущена");
            StartedSession?.Invoke(SessionId);

            return result.Id;
        }

        public async Task JoinToSesstionAsync(string session)
        {
            session = session.Replace("/", "");
            _logger.Info($"Подключение к сессии {session}");
            if (_connection is null)
            {
                throw new Exception("Невозможно подключится к сессии без подключения к серверу.");
            }

            var (result, _) = await _connection.InvokeAsync<ErrorState>(ListenTogetherMethods.JoinPlaySession, new SessionId(session));

            if(!result)
            {
                throw new Exception("Ошибка при подключении к сессии.");
            }

            PlayerMode = PlayerMode.Listener;

            SessionId = session;

            PlaylistTrack currentTrack = null;

            while(currentTrack is null)
            {
                currentTrack = await GetCurrentTrackInSession();
            }

            ConnectedToSession?.Invoke(currentTrack);

            _logger.Info($"Успешное подключение к сессии {session}");

        }

        public async Task ChangeTrackAsync(PlaylistTrack track)
        {
            _logger.Info($"Переключение трека в сессии {SessionId}");

            if (_connection is null)
            {
                throw new Exception("Невозможно выполнить запрос без подключения к серверу.");
            }

            var (result, _) = await _connection.InvokeAsync<ErrorState>(ListenTogetherMethods.ChangeTrack, track);

            if (!result)
            {
                throw new Exception("Ошибка при выполнении запроса.");
            }
        }

        public async Task ChangePlayStateAsync(TimeSpan position, bool pause)
        {
            if (_connection is null)
            {
                throw new Exception("Невозможно выполнить запрос без подключения к серверу.");
            }

            var (result, _) = await _connection.InvokeAsync<ErrorState>(ListenTogetherMethods.ChangePlayState, new PlayState(position, pause));

            if (!result)
            {
                throw new Exception("Ошибка при выполнении запроса.");
            }
        }

        public async Task StopPlaySessionAsync()
        {
            _logger.Info($"Остановка сессии {SessionId}");

            PlayerMode = PlayerMode.None;

            if (_connection is null)
            {
                throw new Exception("Невозможно выполнить запрос без подключения к серверу.");
            }

            var (result, _) = await _connection.InvokeAsync<ErrorState>(ListenTogetherMethods.StopPlaySession);

            if (!result)
            {
                throw new Exception("Ошибка при выполнении запроса.");
            }

            SessionStoped?.Invoke();
        }

        public async Task LeavePlaySessionAsync()
        {
            _logger.Info($"Выход из сессии {SessionId}");

            PlayerMode = PlayerMode.None;

            if (_connection is null)
            {
                throw new Exception("Невозможно выполнить запрос без подключения к серверу.");
            }

            var r = await _connection.InvokeAsync<ErrorState>(ListenTogetherMethods.LeavePlaySession);

            if (!r.Success)
            {
                throw new Exception("Ошибка при выполнении запроса.");
            }

            LeaveSession?.Invoke();
        }

        public async Task LoginAsync(long userId)
        {
            Token = await GetTokenAsync(userId);
        }

        public async Task ConnectToServerAsync(long userId)
        {
            _logger.Info("Подключение к серверу Слушать вместе");

            await GetListenTogetherHostAsync();

            var token = await GetTokenAsync(userId);

            this.Token = token;

            _connection = new HubConnectionBuilder()
                          .WithAutomaticReconnect()
                          .WithUrl($"{Host}/hubs/listen", options =>
                                       options.AccessTokenProvider = () => Task.FromResult(Token)!)
                          .AddProtobufProtocol()
                          .Build();

            SubsribeToCallbacks();

            await _connection.StartAsync();

            _logger.Info("Успешное подключение");

        }

        public async Task<PlaylistTrack> GetCurrentTrackInSession()
            {
            _logger.Info("Получение текущего трека в сессии");

            if (_connection is null) throw new Exception("Сначала необходимо подключится к серверу");

            var result = await _connection.InvokeAsync<PlaylistTrack>(ListenTogetherMethods.GetCurrentTrack);

            return result;
        }

        public async Task<User> GetOwnerSessionInfoAsync()
        {
            _logger.Info("Получение получение информации о владельце сессии.");

            if (_connection is null) throw new Exception("Сначала необходимо подключится к серверу");

            var result = await _connection.InvokeAsync<User>(ListenTogetherMethods.GetSessionOwnerInfo);

            return result;
        }

        public async Task<UsersList> GetListenersInSession()
        {
            _logger.Info("Получение получение информации о списке слушателей.");

            if (_connection is null) throw new Exception("Сначала необходимо подключится к серверу");

            var result = await _connection.InvokeAsync<UsersList>(ListenTogetherMethods.GetListenersInSession);

            return result;
        }


        private async Task<string> GetTokenAsync(long userId)
        {
            _logger.Info("Получение временного токена Слушать вместе");

            using var client = new HttpClient
            {
                BaseAddress = new(Host)
            };
            using var response = await client.PostAsJsonAsync("/token", userId);
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<string>() ?? throw new NullReferenceException("Got null response for token request");

            return token;
        }

        private async Task<string> GetListenTogetherHostAsync()
        {
            if (!string.IsNullOrEmpty(Host))
                return Host;

            try
            {
                _logger.Info("Получение адресса сервера Послушать вместе");
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("https://fooxboy.blob.core.windows.net/musicx/ListenTogetherServers.json");

                    var contents = await response.Content.ReadAsStringAsync();

                    var servers = JsonConvert.DeserializeObject<ListenTogetherServersModel>(contents);

                    return Host =
#if DEBUG
                    servers.Test;
#else
                    servers.Production;
#endif
                }
            }
            catch(Exception)
            {
                return "http://212.192.40.71:5000";
            }
            
        }

        private async Task SessionUserStoped()
        {
            await _connection.StopAsync();
            PlayerMode = PlayerMode.None;
            SessionId = null;
            _connection = null;
        }

        private void SubsribeToCallbacks()
        {
            _subscriptions = new[]
            {
                _connection.On<PlayState>(Callbacks.PlayStateChanged,
                                               (state) => PlayStateChanged?.Invoke(state.Position, state.Pause) ?? Task.CompletedTask),

                _connection.On<PlaylistTrack>(Callbacks.TrackChanged,
                                              (track) => TrackChanged?.Invoke(track) ?? Task.CompletedTask),

                _connection.On<User>(Callbacks.ListenerConnected,
                                               (user) => ListenerConnected?.Invoke(user) ?? Task.CompletedTask),

                _connection.On(Callbacks.SessionStoped, ()=> SessionOwnerStoped?.Invoke() ?? Task.CompletedTask),

                _connection.On<User>(Callbacks.ListenerDisconnected,
                                               (user) => ListenerDisconnected?.Invoke(user) ?? Task.CompletedTask),
            };
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }

            await _connection.StopAsync();
        }
    }
}
