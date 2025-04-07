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
            AudioPlayer.PlayingTrack?.audio;

        public LyricsPage()
        {
            InitializeComponent();
            _geniusService = App._host.Services.GetRequiredService<GeniusService>();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeTimer();
            GeniusToggle.IsChecked = bool.Parse(
                DB.SettingsTable.GetSetting("fromGenius", false.ToString()).settingValue);
            _currentTrack = Track;
            LoadLyricsAsync().ConfigureAwait(false);
        }

        ScrollViewer scrollViewer = null;

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DisposeTimer();
            _disposed = true;
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
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
                timerTickCangeCheck.Invoke(this, new ArgsSeconds((int)AudioPlayer.mediaPlayer.Position.TotalMilliseconds));
            }

            var item = GetLineNumberBySeconds((int)AudioPlayer.mediaPlayer.Position.TotalMilliseconds); 

            var container = ListLyrics.ContainerFromIndex((int)item) as ListViewItem;
            if (container != null)
            {
                var transform = container.TransformToVisual(ListLyrics);
                var position = transform.TransformPoint(new Point(0, 0));
                double itemHeight = position.Y;

                //scrollViewer.ChangeView(null, , null);
                this.ScrollToElement(item, itemHeight);
            }
        }

        public void ScrollToElement(int element, double point)
        {
            if (scrollViewer == null)
            {
                scrollViewer = SmallHelpers.FindScrollViewer(ListLyrics);
            }

            var ins = element;


            if (ins >= 0 && ins < ListLyrics.Items.Count)
            {
                var container = ListLyrics.ContainerFromIndex(ins) as ListViewItem;
                if (container != null)
                {
                    var transform = container.TransformToVisual(ListLyrics);
                    var position = transform.TransformPoint(new Point(0, 0));
                    double itemHeight = position.Y;

                    scrollViewer.ChangeView(null, scrollViewer.VerticalOffset + itemHeight - 5, null);
                }
            }
        }

        // Метод для получения номера строки по секундам
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
            if (Track == null)
                return;

            var loadId = Guid.NewGuid();
            _currentLoadId = loadId;

            try
            {
                IsLoading = true;
                Texts.Clear();

                bool result;

                if (GeniusToggle.IsChecked == true)
                {
                    result = await TryLoadGeniusLyrics(Track, loadId);
                    if (!result && _currentLoadId == loadId)
                        AddTextLines(new[] { "Текст песни не найден в Genius" }, loadId);
                }
                else
                {
                    result = await TryLoadVkLyrics(Track, loadId);
                    if (!result && _currentLoadId == loadId)
                        AddTextLines(new[] { "Текст песни не найден" }, loadId);
                }
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
                    IsLoading = false;
            }
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

        private void AddTextLines(IEnumerable<object> lines, Guid loadId)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                // Добавляем только если это последний запрос
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

        private async void GeniusToggle_Toggled(object sender, RoutedEventArgs e)
        {
            DB.SettingsTable.SetSetting("fromGenius", GeniusToggle.IsChecked.ToString());
            await LoadLyricsAsync();
        }

        public void Enable()
        {
            _timer?.Start();
        }

        public void Disable()
        {
            _timer?.Stop();
        }
    }
}