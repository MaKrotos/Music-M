using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Shared.Player;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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


        public ObservableRangeCollection<object> Texts { get; set; } = new ObservableRangeCollection<object>();

        public event Action<int> NextLineEvent;

        public event Action NewTrack;

        public event EventHandler closeClicked;

        public VkNet.Model.Attachments.Audio? Track
        {
            get
            {
                if (AudioPlayer.PlayingTrack == null)
                    return null;
                return AudioPlayer.PlayingTrack.audio;
            }
        }

        private DispatcherTimer _timer { get; set; }


        public LyricsPage()
        {
            this.InitializeComponent();
            this.Loaded += LyricsPage_Loaded;
        }

        private void LyricsPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.GeniusToggle.IsChecked = bool.Parse(DB.SettingsTable.GetSetting("fromGenius", false.ToString()).settingValue);
            LoadLyrics();

            if (_timer is null)
            {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(500);
                _timer.Tick += Timer_Tick;
            }
        }

        public void disable()
        {

            _timer.Stop();
        }
        public void enable()
        {
            _timer.Start();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

        }

        public async Task LoadLyrics()
        {
            try
            {
                if (Track == null)
                    return;

                IsLoading = true;
                bool result = false;


                result = (bool) GeniusToggle.IsChecked
                    ? await LoadGenius() || await LoadLyricFind(Track)
                    : await LoadLyricFind(Track) || await LoadGenius();


                if (!result)
                    return;

                

                IsLoading = false;
            }
            catch (Exception ex)
            {

                Texts = new ObservableRangeCollection<object>() { "Ошибка загрузки" };
                IsLoading = false;
            }
        }
        VkNet.Model.Attachments.Audio? TempTrack = null;
        private void Timer_Tick(object sender, object e)
        {
            if (TempTrack != Track)
            {
                TempTrack = Track;
                LoadLyrics();
            }
        }

        private async Task<bool> LoadLyricFind(VkNet.Model.Attachments.Audio track)
        {
            Texts.Clear();

            if (!track.HasLyrics)
            {
                Texts = new ObservableRangeCollection<object>() { "Этот трек", "Не имеет текста" };
                IsLoading = false;

                return false;
            }

            Lyrics vkLyrics = await VK.vkService.GetLyrics(track.OwnerId + "_" + track.Id);

            if (vkLyrics.LyricsInfo.Timestamps != null)
            {
                Texts.AddRange(vkLyrics.LyricsInfo.Timestamps);
            }
            else
            {
                Texts.AddRange(vkLyrics.LyricsInfo.Text);
            }
            Credits = vkLyrics.Credits;

            return true;
        }

        private async Task<bool> LoadGenius()
        {
            Texts.Clear();

            var hits = await _geniusService.SearchAsync($"{Track.Title} {Track.MainArtists.First().Name}");

            if (!hits.Any())
            {
                Texts = new ObservableRangeCollection<object>() { "Этот трек", "Не имеет текста" };
                IsLoading = false;

                return false;
            }

            var song = await _geniusService.GetSongAsync(hits.First().Result.Id);

            Texts.AddRange(song.Lyrics.Plain.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList());

            Credits = "Текст предоставлен Genius (genius.com).\nВКонтакте и MusicX к данному сервису, а также к содержанию текста отношения не имеют.";

            return true;
        }

        private void GeniusToggle_Toggled(object sender, RoutedEventArgs e)
        {
            DB.SettingsTable.SetSetting("fromGenius", GeniusToggle.IsChecked.ToString());
            LoadLyrics();
        }

        
    }
}
