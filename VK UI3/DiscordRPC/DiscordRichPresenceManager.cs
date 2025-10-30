using DiscordRPC;
using System;
using System.Threading;
using VK_UI3.Helpers;
using Windows.Media.Playback;
using Windows.System.Profile;

namespace VK_UI3.DiscordRPC
{
    public class DiscordRichPresenceManager : IDisposable
    {
        private readonly DiscordRpcClient _client;
        private Timer _timer;
        private Timer _reconnectTimer;
        private int _currentSecond;
        private string _currentTrack;
        private string _currentArtist;
        private const int ReconnectDelay = 5000; 

        public DiscordRichPresenceManager()
        {
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

            InitializeClient();
        }

        private void InitializeClient()
        {
            try
            {
                if (!_client.IsInitialized)
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
            _reconnectTimer?.Dispose(); // Останавливаем предыдущий таймер, если он был
            _reconnectTimer = new Timer(_ =>
            {
                Console.WriteLine("Attempting to reconnect to Discord...");
                InitializeClient();
            }, null, ReconnectDelay, Timeout.Infinite); // Повторная попытка через ReconnectDelay миллисекунд
        }

        string image;
        string imageSmall;

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
                if (_client.IsInitialized)
                {
                    var presence = new RichPresence()
                    {
                        Details = $"{_currentTrack}",
                        State = $"{_currentArtist}",
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
                            SmallImageKey = imageSmall,
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
                else
                {
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Останавливает таймер и освобождает ресурсы.
        /// </summary>
        public void Dispose()
        {
            _timer?.Dispose();
            _reconnectTimer?.Dispose();
            _client.Dispose();
        }

        MediaPlayer media;

        internal void SetTrack(ExtendedAudio trackDataThis, MediaPlayer mediaPlayer)
        {
            try
            {
                _currentTrack = trackDataThis.audio.Title;
                _currentArtist = trackDataThis.audio.Artist ?? "Неизвестен";
                media = mediaPlayer;

             
                _timer?.Dispose(); 
                _timer = new Timer(UpdatePresence, null, 0, 5000);


                if (trackDataThis != null && trackDataThis.audio != null && trackDataThis.audio.Album != null && trackDataThis.audio.Album.Thumb != null)
                {
                    this.image = 
                        trackDataThis.audio.Album.Thumb.Photo68 ??
                        trackDataThis.audio.Album.Thumb.Photo300 ??
                        trackDataThis.audio.Album.Thumb.Photo1200 ??
                        trackDataThis.audio.Album.Thumb.Photo1200 ??
                        trackDataThis.audio.Album.Thumb.Photo600 ??
                        trackDataThis.audio.Album.Thumb.Photo270 ??
                        null;
                    ///this.imageSmall = imageSmall;
                }
            }
            catch (Exception ex)
            {

            }
            update();
        }
    }
}