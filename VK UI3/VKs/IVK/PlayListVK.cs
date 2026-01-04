using Microsoft.UI.Dispatching;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using VkNet.Model.Attachments;

namespace VK_UI3.VKs.IVK
{
    public class PlayListVK : IVKGetAudio, IVKGetTracksMore
    {
        public AudioPlaylist playlist;
        public string _Year;
        public string _Description;
        public string genres;
        public string artists;

        public string Plays { get; private set; }

        public PlayListVK(AudioPlaylist _playlist, DispatcherQueue dispatcher) : base(dispatcher)
        {
            this.playlist = _playlist;
            getLoadedTracks = true;
            DispatcherQueue = dispatcher;
            Task.Run(async () =>
            {
                try
                {
                    var p = await VK.vkService.GetPlaylistAsync(100, _playlist.Id, _playlist.AccessKey, _playlist.OwnerId);

                    if ((p.Playlist.MainArtists == null || p.Playlist.MainArtists.Count == 0) && p.Playlist.OwnerId < 0 && p.Groups != null)
                    {
                        p.Playlist.OwnerName = p.Groups[0].Name;
                    }

                    playlist = p.Playlist;
                    playlist.groupOwner = _playlist.groupOwner;
                    playlist.userOwner = _playlist.userOwner;
                    playlist.OwnerName = _playlist.OwnerName;
                    playlist.Audios = new ReadOnlyCollection<VkNet.Model.Attachments.Audio>(p.Audios.Cast<VkNet.Model.Attachments.Audio>().ToList());

                    name = playlist.Title;
                    _Year = playlist.Year.ToString();
                    _Description = playlist.Description;

                    genres = string.Empty;

                    foreach (var genre in playlist.Genres)
                    {
                        genres += $"{genre.Name}, ";
                    }

                    if (playlist.Genres.Count > 0)
                    {
                        genres = genres.Remove(genres.Length - 2);
                    }
                    countTracks = playlist.Count;
                    countUpdated();

                    if (playlist.Cover != null)
                    {
                        photoUri = new Uri(playlist.Cover);
                        PhotoUpdated();
                    }

                    if (playlist.Year == 0)
                    {
                        var date = playlist.UpdateTime;
                        _Year = $"Обновлен {date.ToString("dd MMMM")}";
                        genres = "Подборка";
                    }

                    if (playlist.MainArtists != null && playlist.MainArtists.Count > 0)
                    {
                        string s = string.Empty;
                        foreach (var trackArtist in playlist.MainArtists)
                        {
                            s += trackArtist.Name + ", ";
                        }

                        var artists = s.Remove(s.Length - 2);

                        artists = artists;
                    }
                    else
                    {
                        artists = playlist.OwnerName;
                    }
                    countTracks += listAudio.Count;

                    listAudio.AddRange(playlist.Audios.Select(item => new ExtendedAudio(item, this)));

                    if (playlist.Audios.Count == 0)
                    {
                        var res = await VK.vkService.AudioGetAsync(playlist.Id, playlist.OwnerId, playlist.AccessKey).ConfigureAwait(false);
                        listAudio.AddRange(res.Items.Select(item => new ExtendedAudio(item, this)));
                    }

                    if (playlist.Plays > 1000000)
                    {
                        Plays = Math.Round(playlist.Plays / 1000000d, 2) + "М";
                    }
                    else if (playlist.Plays > 1000)
                    {
                        Plays = Math.Round(playlist.Plays / 1000d, 2) + "К";
                    }
                    else
                    {
                        Plays = playlist.Plays.ToString();
                    }
                }
                catch (Exception e)
                {
                }
                finally
                {
                }
                getLoadedTracks = false;
                NotifyOnListUpdate();
            });
        }

        public PlayListVK(RecommendedPlaylist _playlist, DispatcherQueue dispatcher) : base(dispatcher)
        {
            playlist = _playlist.Playlist;
            foreach (var audio in _playlist.Audios)
            {
                listAudio.Add(new ExtendedAudio(audio, this));
            }
            countTracks = playlist.Count;

            _Description = playlist.Description;
        }

        public override long? getCount()
        {
            return null;
        }

        public override string getName()
        {
            return null;
        }

        public override Uri getPhoto()
        {
            return null;
        }

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public override void GetTracks()
        {
            semaphore.Wait(); // Ожидает освобождения семафора

            try
            {
                if (getLoadedTracks) return;

                if (listAudio.Count >= playlist.Count || itsAll)
                {
                    itsAll = true;
                    NotifyOnListUpdate();
                    return;
                }

                getLoadedTracks = true;

                task = Task.Run(async () =>
                {
                    int offset = listAudio.Count;
                    int count = 100;

                    if (countTracks > listAudio.Count)
                    {
                        var response = await VK.vkService.AudioGetAsync(playlist.Id, playlist.OwnerId, playlist.AccessKey, listAudio.Count, count);

                        foreach (var item in response.Items)
                        {
                            ExtendedAudio extendedAudio = new ExtendedAudio(item, this);
                            ManualResetEvent resetEvent = new ManualResetEvent(false);

                            DispatcherQueue.TryEnqueue(() =>
                            {
                                listAudio.Add(extendedAudio);
                                resetEvent.Set();
                            });

                            resetEvent.WaitOne();
                        }

                        if (countTracks == listAudio.Count()) itsAll = true;

                        getLoadedTracks = false;
                    }
                    NotifyOnListUpdate();
                    task = null;
                });
            }
            finally
            {
                semaphore.Release(); // Освобождает семафор
            }
        }

        public override List<string> getPhotosList()
        {
            List<string> list = new List<string>();
            if (playlist.Cover != null)
                list.Add(playlist.Cover);
            else
            {
                if (playlist.Thumbs != null)
                    foreach (var item in playlist.Thumbs)
                    {
                        list.Add(
                            item.Photo600 ??
                            item.Photo1200 ??
                            item.Photo300 ??
                            item.Photo34 ??
                            item.Photo270 ??
                            item.Photo135 ??
                            item.Photo68
                            );
                    }
            }
            return list;
        }

        public void GetTracks(int counter)
        {
            semaphore.Wait(); 

            try
            {
                if (getLoadedTracks) return;

                if (listAudio.Count >= playlist.Count || itsAll)
                {
                    itsAll = true;
                    NotifyOnListUpdate();
                    return;
                }

                getLoadedTracks = true;

                task = Task.Run(async () =>
                {
                    int offset = listAudio.Count;
                    int count = counter;

                    if (countTracks > listAudio.Count)
                    {
                        var response = await VK.vkService.AudioGetAsync(playlist.Id, playlist.OwnerId, playlist.AccessKey, listAudio.Count, count);

                        foreach (var item in response.Items)
                        {
                            ExtendedAudio extendedAudio = new ExtendedAudio(item, this);
                            ManualResetEvent resetEvent = new ManualResetEvent(false);

                            DispatcherQueue.TryEnqueue(() =>
                            {
                                listAudio.Add(extendedAudio);
                                resetEvent.Set();
                            });

                            resetEvent.WaitOne();
                        }

                        if (countTracks == listAudio.Count()) itsAll = true;

                        getLoadedTracks = false;
                    }
                    NotifyOnListUpdate();
                    task = null;
                });
            }
            finally
            {
                semaphore.Release(); // Освобождает семафор
            }
        }
    }
}
