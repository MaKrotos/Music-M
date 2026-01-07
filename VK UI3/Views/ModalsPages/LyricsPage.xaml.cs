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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.Helpers;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using Windows.Foundation;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Windows.ApplicationModel.DataTransfer;

namespace VK_UI3.Views.ModalsPages
{
    public class ArgsSeconds : EventArgs
    {
        public ArgsSeconds(int mssecond)
        {
            this.mssecond = mssecond;
        }

        public int mssecond { get; set; }
    }

    public sealed partial class LyricsPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly GeniusService _geniusService;
        private readonly HttpClient _httpClient;
        private VkNet.Model.Attachments.Audio _currentTrack;
        private DispatcherTimer _timer;
        private bool _disposed;

        public static event EventHandler timerTickCangeCheck;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        private bool _isLoading;

        public string Credits
        {
            get => _credits;
            set => SetProperty(ref _credits, value);
        }
        private string _credits;

        public MvvmHelpers.ObservableRangeCollection<object> Texts { get; } = new MvvmHelpers.ObservableRangeCollection<object>();

        public VkNet.Model.Attachments.Audio Track =>
            VK_UI3.Services.MediaPlayerService.PlayingTrack?.audio;

        public LyricsPage()
        {
            InitializeComponent();
            _geniusService = App._host.Services.GetRequiredService<GeniusService>();
            _httpClient = new HttpClient();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            MoveBackDownStoryboard.Completed += MoveBackDownStoryboard_Completed;
            // Initialize source toggle
            GeniusToggle.Items.Add("Auto");
            GeniusToggle.Items.Add("VK");
            GeniusToggle.Items.Add("Genius");
            GeniusToggle.Items.Add("LRCLib");
            GeniusToggle.SelectedIndex = int.Parse(
            DB.SettingsTable.GetSetting("lyricsSource", "0").settingValue);
        }

        private void MoveBackDownStoryboard_Completed(object sender, object e)
        {
            if (GridBRing.Opacity != 0)
                return;

            GridBRing.Visibility = Visibility.Collapsed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeTimer();
            _currentTrack = Track;

            if (scrollViewer == null)
            {
                scrollViewer = SmallHelpers.FindScrollViewer(ListLyrics);
                if (scrollViewer != null)
                {
                    scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
                }
            }

            LoadLyricsAsync().ConfigureAwait(false);
        }

        private bool _isUserScrolling = false;
        private DateTime _lastUserScrollTime;

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (!_isProgrammaticScroll)
            {
                _isUserScrolling = true;
                _lastUserScrollTime = DateTime.Now;
                DisableAutoScrollTemporarily();
            }
        }

        private bool _isProgrammaticScroll = false;

        private async void DisableAutoScrollTemporarily()
        {
            disabledAutoScroll = true;
            await Task.Delay(5000);

            if ((DateTime.Now - _lastUserScrollTime).TotalSeconds >= 5)
            {
                disabledAutoScroll = false;
                _isUserScrolling = false;
            }
        }

        ScrollViewer scrollViewer = null;

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (scrollViewer != null)
            {
                scrollViewer.ViewChanging -= ScrollViewer_ViewChanging;
            }
            DisposeTimer();
            _disposed = true;
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += OnTimerTick;
        }

        private void DisposeTimer()
        {
            if (_timer == null)
                return;

            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }

        private async void OnTimerTick(object sender, object e)
        {
            if (_disposed || Track == null)
                return;

            if (_currentTrack?.Id != Track.Id || _currentTrack?.OwnerId != Track.OwnerId)
            {
                _currentTrack = Track;
                await LoadLyricsAsync();
            }

            if (timerTickCangeCheck != null)
            {
                timerTickCangeCheck.Invoke(this, new ArgsSeconds((int)VK_UI3.Services.MediaPlayerService.MediaPlayer.Position.TotalMilliseconds));
            }

            if (disabledAutoScroll)
                return;

            var item = GetLineNumberBySeconds((int)VK_UI3.Services.MediaPlayerService.MediaPlayer.Position.TotalMilliseconds);

            var container = ListLyrics.ContainerFromIndex((int)item) as ListViewItem;
            if (container != null)
            {
                var transform = container.TransformToVisual(ListLyrics);
                var position = transform.TransformPoint(new Point(0, 0));
                double itemHeight = position.Y;
                this.ScrollToElement(item, itemHeight);
            }
        }

        public void ScrollToElement(int element, double point)
        {
            if (scrollViewer == null)
            {
                scrollViewer = SmallHelpers.FindScrollViewer(ListLyrics);
                scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            }

            var ins = element;

            if (ins >= 0 && ins < ListLyrics.Items.Count)
            {
                _isProgrammaticScroll = true;
                var container = ListLyrics.ContainerFromIndex(ins) as ListViewItem;
                if (container != null)
                {
                    var transform = container.TransformToVisual(ListLyrics);
                    var position = transform.TransformPoint(new Point(0, 0));
                    double itemHeight = position.Y;

                    scrollViewer.ChangeView(null, scrollViewer.VerticalOffset + itemHeight - 50, null);
                }

                Task.Delay(500).ContinueWith(_ =>
                {
                    DispatcherQueue.TryEnqueue(() => _isProgrammaticScroll = false);
                });
            }
        }

        public int GetLineNumberBySeconds(int seconds)
        {
            for (int i = 0; i < Texts.Count(); i++)
            {
                if (Texts[i] is LyricsTimestamp timestamp)
                {
                    if (seconds >= timestamp.Begin && seconds <= timestamp.End)
                    {
                        return i;
                    }
                }
                else
                    return -1;
            }
            return -1;
        }

        private Guid _currentLoadId;

        public async Task LoadLyricsAsync()
        {
            ShowLoadRing();
            if (Track == null)
                return;

            var loadId = Guid.NewGuid();
            _currentLoadId = loadId;

            try
            {
                IsLoading = true;
                Texts.Clear();

                bool result = false;

                switch (GeniusToggle.SelectedIndex)
                {
                    case 0:  //Auto
                        result = await TryLoadVkLyrics(Track, loadId)
                            || await TryLoadLrcLibLyrics(Track, loadId)
                            || await TryLoadGeniusLyrics(Track, loadId);
                        break;
                    case 1: // VK
                        result = await TryLoadVkLyrics(Track, loadId);
                        break;
                    case 2: // Genius                
                        result = await TryLoadGeniusLyrics(Track, loadId);
                        break;
                    case 3:  // LRCLib
                        result = await TryLoadLrcLibLyrics(Track, loadId);
                        break;
                }

                if (!result && _currentLoadId == loadId)
                    AddTextLines(new[] { "Текст песни не найден" }, loadId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки: {ex}");
                if (_currentLoadId == loadId)
                    AddTextLines(new[] { "Ошибка загрузки текста" }, loadId);
            }
            finally
            {
                if (_currentLoadId == loadId)
                {
                    IsLoading = false;
                    hideLoadRing();
                }
            }
        }

        private void hideLoadRing()
        {
            MoveBackUpStoryboard.Pause();
            MoveBackDownStoryboard.Begin();
        }

        private void ShowLoadRing()
        {
            GridBRing.Visibility = Visibility.Visible;
            MoveBackDownStoryboard.Pause();
            MoveBackUpStoryboard.Begin();
        }

        private async Task<bool> TryLoadVkLyrics(VkNet.Model.Attachments.Audio track, Guid loadId)
        {
            if (!track.HasLyrics || _currentLoadId != loadId)
                return false;

            try
            {
                var vkLyrics = await VK.vkService.GetLyrics($"{track.OwnerId}_{track.Id}");

                if (vkLyrics?.LyricsInfo == null || _currentLoadId != loadId)
                    return false;

                var content = vkLyrics.LyricsInfo.Timestamps ??
                             vkLyrics.LyricsInfo.Text.Cast<object>();

                if (_currentLoadId == loadId)
                {
                    AddTextLines(content, loadId);
                    Credits = vkLyrics.Credits;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryLoadGeniusLyrics(VkNet.Model.Attachments.Audio track, Guid loadId)
        {
            if (_currentLoadId != loadId)
                return false;

            try
            {
                var artist = track.MainArtists?.FirstOrDefault()?.Name ?? "";
                var results = await _geniusService.SearchAsync($"{track.Title} {artist}");
                var song = results.FirstOrDefault()?.Result;

                if (song == null || _currentLoadId != loadId)
                    return false;

                var lyrics = await _geniusService.GetSongAsync(song.Id);
                var lines = lyrics.Lyrics.Plain.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                if (_currentLoadId == loadId)
                {
                    AddTextLines(lines, loadId);
                    Credits = "Текст предоставлен Genius (genius.com)";
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryLoadLrcLibLyrics(VkNet.Model.Attachments.Audio track, Guid loadId)
        {
            if (_currentLoadId != loadId)
                return false;

            try
            {
                var artist = track.MainArtists?.FirstOrDefault()?.Name ?? "";
                var title = track.Title;

                // Remove (feat. ...) parts from title if present
                var featIndex = title.IndexOf("(feat.");
                if (featIndex > 0)
                {
                    title = title.Substring(0, featIndex).Trim();
                }

                var url = $"https://lrclib.net/api/search?track_name={WebUtility.UrlEncode(title)}&artist_name={WebUtility.UrlEncode(artist)}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var results = JsonSerializer.Deserialize<List<LrcLibResponse>>(content, options);

                var bestMatch = results?.FirstOrDefault();
                if (bestMatch == null || _currentLoadId != loadId)
                    return false;

                var lines = ParseLrcText(bestMatch.SyncedLyrics ?? bestMatch.PlainLyrics);

                if (_currentLoadId == loadId)
                {
                    AddTextLines(lines, loadId);
                    Credits = "Текст предоставлен LRCLib (lrclib.net)";
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private List<object> ParseLrcText(string lrcText)
        {
            var result = new List<object>();

            if (string.IsNullOrEmpty(lrcText))
                return result;

            var lines = lrcText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            LyricsTimestamp previousItem = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                if (TryParseLrcLine(trimmedLine, out var timestamp, out var text))
                {
                    var begin = (int)timestamp.TotalMilliseconds;

                    if (previousItem != null)
                    {
                        previousItem.End = begin;
                    }

                    var currentItem = new LyricsTimestamp
                    {
                        Line = text ?? string.Empty, // Handle null text
                        Begin = begin,
                        End = begin + 5000 // Default duration
                    };

                    result.Add(currentItem);
                    previousItem = currentItem;
                }
                else
                {
                    if (previousItem != null)
                    {
                        previousItem.End = previousItem.Begin + 5000;
                        previousItem = null;
                    }

                    result.Add(trimmedLine);
                }
            }

            return result;
        }

        private bool TryParseLrcLine(string line, out TimeSpan timestamp, out string text)
        {
            timestamp = TimeSpan.Zero;
            text = null;

            if (!line.StartsWith("[") || line.IndexOf(']') <= 1)
                return false;

            var closeBracketIndex = line.IndexOf(']');
            var timePart = line.Substring(1, closeBracketIndex - 1);

            // Поддержка форматов: mm:ss.ff, mm:ss.fff, m:ss.ff, m:ss.fff
            string[] formats = { @"m\:ss\.ff", @"m\:ss\.fff", @"mm\:ss\.ff", @"mm\:ss\.fff" };

            bool parsed = false;
            foreach (var format in formats)
            {
                if (TimeSpan.TryParseExact(timePart, format, null, out timestamp))
                {
                    parsed = true;
                    break;
                }
            }

            if (!parsed)
                return false;

            // Получить текст после временной метки (если есть)
            if (closeBracketIndex + 1 < line.Length)
            {
                text = line.Substring(closeBracketIndex + 1).Trim();
            }
            else
            {
                text = string.Empty;
            }

            return true;
        }


        private void AddTextLines(IEnumerable<object> lines, Guid loadId)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_currentLoadId == loadId)
                {
                    Texts.Clear();
                    Texts.AddRange(lines);
                }
            });
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void LyricsSourceToggle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DB.SettingsTable.SetSetting("lyricsSource", GeniusToggle.SelectedIndex.ToString());
            await LoadLyricsAsync();
        }

        bool disabledAutoScroll = false;

        public void Enable()
        {
            _timer?.Start();
        }

        public void Disable()
        {
            _timer?.Stop();
        }

        private void GeniusToggle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ = LoadLyricsAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainView.mainView.ToggleLyricsPanel();
        }

        private void CopyClipboard_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder textToClip = new StringBuilder(); ;
            foreach (var item in Texts)
            {

                if (item is string TextString)
                {
                    textToClip.AppendLine(TextString);
                }
                else if (item is LyricsTimestamp timeText)
                {
                    textToClip.AppendLine(timeText.Line);
                }
            }

            var package = new DataPackage();
            package.SetText(textToClip.ToString());
            Clipboard.SetContent(package);

        }
    }


    public class LrcLibResponse
    {
        public string TrackName { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public string PlainLyrics { get; set; }
        public string SyncedLyrics { get; set; }
    }
}