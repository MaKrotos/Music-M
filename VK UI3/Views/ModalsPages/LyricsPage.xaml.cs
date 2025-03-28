using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Shared.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.VKs;


namespace VK_UI3.Views.ModalsPages
{




    public sealed partial class LyricsPage : Page
    {

        public static readonly GeniusService _geniusService = App._host.Services.GetRequiredService<GeniusService>();

        public bool IsLoading { get; set; }

        public string Credits { get; set; }

        public List<LyricsTimestamp> Timestamps { get; set; }

        public List<string> Texts { get; set; }

        public event Action<int> NextLineEvent;

        public event Action NewTrack;

        public VkNet.Model.Attachments.Audio Track { get { return AudioPlayer.PlayingTrack.audio; } }

        private DispatcherTimer _timer { get; set; }

        public LyricsPage()
        {
            this.InitializeComponent();
            this.Loaded += LyricsPage_Loaded;
        }

        private void LyricsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLyrics(true);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

        }

        public async Task LoadLyrics(bool isGenius)
        {
            try
            {
                IsLoading = true;
                

                var result = isGenius ? await LoadGenius() : await LoadLyricFind(Track);
                if (!result)
                    return;

                if (_timer is null)
                {
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromMilliseconds(500);
                    _timer.Tick += Timer_Tick;
                    _timer.Start();
                }

                IsLoading = false;
            }
            catch (Exception ex)
            {

                Texts = new List<string>() { "Ошибка загрузки" };
                IsLoading = false;
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            try
            {
                //     var currentPositionOnMs = _playerService.Position.TotalMilliseconds;

                //     NextLineEvent?.Invoke(Convert.ToInt32(currentPositionOnMs));

            }
            catch (Exception ex)
            {
                //   _logger.Error(ex, "Failed to jump to next lyrics line");
            }
        }

        private async Task<bool> LoadLyricFind(VkNet.Model.Attachments.Audio track)
        {
            if (!track.HasLyrics)
            {
                Texts = new List<string>() { "Этот трек", "Не имеет текста" };
                IsLoading = false;

                return false;
            }

            var vkLyrics = await VK.vkService.GetLyrics(track.OwnerId + "_" + track.Id);

            Timestamps = vkLyrics.LyricsInfo.Timestamps;

            if (Timestamps is null)
            {
                Texts = vkLyrics.LyricsInfo.Text;
            }

            Credits = vkLyrics.Credits;

            return true;
        }

        private async Task<bool> LoadGenius()
        {
            var hits = await _geniusService.SearchAsync($"{Track.Title} {Track.MainArtists.First().Name}");

            if (!hits.Any())
            {
                Texts = new List<string>() { "Этот трек", "Не имеет текста" };
                IsLoading = false;

                return false;
            }

            var song = await _geniusService.GetSongAsync(hits.First().Result.Id);

            Texts = song.Lyrics.Plain.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            Credits = "Текст предоставлен Genius (genius.com).\nВКонтакте и MusicX к данному сервису, а также к содержанию текста отношения не имеют.";

            return true;
        }

    }
}
