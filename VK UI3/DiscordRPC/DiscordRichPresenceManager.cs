using DiscordRPC;
using System;
using System.Threading;
using VK_UI3.Helpers;
using Windows.Media.Playback;

namespace VK_UI3.DiscordRPC
{
    public class DiscordRichPresenceManager : IDisposable
    {
        private DiscordRpcClient _client;
        private Timer _timer;
        private Timer _reconnectTimer;
        private string _currentTrack;
        private string _currentArtist;
        private const int ReconnectDelay = 5000;
        private const int UpdateInterval = 5000;
        private readonly object _lock = new object();
        private bool _disposed;

        public DiscordRichPresenceManager()
        {
            CreateClient();
            InitializeClient();
        }

        private void CreateClient()
        {
            _client?.Dispose();
            _client = new DiscordRpcClient("1350750411811983440");

            _client.OnReady += (sender, e) =>
            {
                Console.WriteLine($"Connected to Discord as {e.User.Username}");
            };

            _client.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine($"Presence updated: {e.Presence}");
            };

            _client.OnError += (sender, e) =>
            {
                Console.WriteLine($"An error occurred: {e.Message}");
                AttemptReconnect();
            };

            _client.OnClose += (sender, e) =>
            {
                Console.WriteLine("Connection to Discord closed.");
                AttemptReconnect();
            };
        }

        private void InitializeClient()
        {
            try
            {
                if (!_client.IsInitialized && !_client.IsDisposed)
                {
                    _client.Initialize();
                    Console.WriteLine("Discord RPC initialized successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize Discord RPC: {ex.Message}");
                AttemptReconnect();
            }
        }

        private void AttemptReconnect()
        {
            lock (_lock)
            {
                if (_disposed) return;

                _reconnectTimer?.Dispose();
                _reconnectTimer = new Timer(_ =>
                {
                    lock (_lock)
                    {
                        if (_disposed) return;
                        Console.WriteLine("Attempting to reconnect to Discord...");
                        CreateClient();
                        InitializeClient();
                    }
                }, null, ReconnectDelay, Timeout.Infinite);
            }
        }

        string image;

        /// <summary>
        /// Обновляет статус в Discord.
        /// </summary>
        private void UpdatePresence(object state)
        {
            update();
        }

        private void update()
        {
            try
            {
                lock (_lock)
                {
                    if (_disposed || !_client.IsInitialized || media == null)
                        return;

                    var presence = new RichPresence()
                    {
                        Details = _currentTrack ?? "Неизвестный трек",
                        State = _currentArtist ?? "Неизвестен",
                        Type = ActivityType.Listening,
                        Timestamps = new Timestamps()
                        {
                            Start = DateTime.UtcNow.AddSeconds(-media.Position.TotalSeconds),
                            End = DateTime.UtcNow.AddSeconds(media.NaturalDuration.TotalSeconds - media.Position.TotalSeconds)
                        },
                        Assets = new Assets()
                        {
                            LargeImageKey = image,
                            LargeImageText = "VK Music",
                            SmallImageKey = null,
                            SmallImageText = "Слушает"
                        },
                        Buttons = new Button[]
                        {
                            new Button()
                            {
                                Label = "Слушать в VK M",
                                Url = "https://github.com/MaKrotos/Music-M"
                            },
                            new Button()
                            {
                                Label = "ТГ Канал",
                                Url = "https://t.me/VK_M_creator"
                            }
                        }
                    };
                    _client.SetPresence(presence);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Discord presence: {ex.Message}");
            }
        }

        /// <summary>
        /// Останавливает таймер и освобождает ресурсы.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;

                _timer?.Dispose();
                _timer = null;

                _reconnectTimer?.Dispose();
                _reconnectTimer = null;

                try
                {
                    if (_client.IsInitialized)
                    {
                        _client.ClearPresence();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error clearing Discord presence: {ex.Message}");
                }

                _client.Dispose();
                _client = null;
                media = null;
            }
        }

        MediaPlayer media;

        internal void SetTrack(ExtendedAudio trackDataThis, MediaPlayer mediaPlayer)
        {
            try
            {
                lock (_lock)
                {
                    if (_disposed) return;

                    _currentTrack = trackDataThis?.audio?.Title ?? "Неизвестный трек";
                    _currentArtist = trackDataThis?.audio?.Artist ?? "Неизвестен";
                    media = mediaPlayer;

                    // Обновляем изображение альбома
                    if (trackDataThis?.audio?.Album?.Thumb != null)
                    {
                        var thumb = trackDataThis.audio.Album.Thumb;
                        string photoUrl = thumb.Photo68 ?? thumb.Photo300 ?? thumb.Photo600 ?? thumb.Photo270 ?? thumb.Photo1200;
                        if (!string.IsNullOrEmpty(photoUrl))
                        {
                            // Discord требует формат mp:external/ для внешних изображений
                            this.image = $"mp:external/{photoUrl}";
                        }
                        else
                        {
                            this.image = null;
                        }
                    }
                    else
                    {
                        this.image = null;
                    }
                }

                // Перезапускаем таймер обновления
                _timer?.Dispose();
                _timer = new Timer(UpdatePresence, null, 0, UpdateInterval);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting Discord track: {ex.Message}");
            }
        }
    }
}