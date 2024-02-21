using Microsoft.UI.Dispatching;
using MusicX.Core.Models;
using MusicX.Core.Models.Genius;
using MusicX.Core.Services;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Model.RequestParams.Leads;
using VkNet.Utils;

namespace VK_UI3.VKs.IVK
{
    public class PlayListVK : IVKGetAudio
    {
        Playlist playlist;
        public string _Year;
        public string _Description;
        public string genres;
        public string artists;

        public string Plays { get; private set; }

        public PlayListVK(Playlist _playlist, DispatcherQueue dispatcher) : base(dispatcher)
        {
            Task.Run(async () =>
            {

                try {

                    var p = await VK.vkService.GetPlaylistAsync(100, _playlist.Id, _playlist.AccessKey, _playlist.OwnerId);

                    if (p.Playlist.MainArtists.Count == 0)
                    {
                        if (p.Playlist.OwnerId < 0)
                        {
                            if (p.Groups != null)
                            {
                                p.Playlist.OwnerName = p.Groups[0].Name;

                            }
                        }
                    }
                    playlist = p.Playlist;
                    playlist.Audios = p.Audios;
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        foreach (var item in playlist.Audios)
                        {
                            listAudio.Add(new ExtendedAudio(item, this));
                        }
                    });
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
                        var date = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(playlist.UpdateTime);
                        _Year = $"Обновлен {date.ToString("dd MMMM")}";
                        genres = "Подборка";
                    }

                    if (playlist.MainArtists.Count > 0)
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

                    if (playlist.Audios.Count == 0)
                    {

                        var res = await VK.vkService.AudioGetAsync(playlist.Id, playlist.OwnerId, playlist.AccessKey).ConfigureAwait(false);

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            foreach (var item in res.Items)
                            {
                                listAudio.Add(new ExtendedAudio(item, this));
                            }
                        });
                    }



                    if (playlist.Plays > 1000 && playlist.Plays < 1000000)
                    {
                        Plays = Math.Round(playlist.Plays / 1000d, 2) + "К";
                    }
                    else if (playlist.Plays > 1000000)
                    {
                        Plays = Math.Round(playlist.Plays / 1000000d, 2) + "М";

                    }
                    else
                    {
                        Plays = playlist.Plays.ToString();
                    }


                }
                catch 
                (Exception e)
                { }


                NotifyOnListUpdate();
            });
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

        public override void GetTracks()
        {
            if (getLoadedTracks) return;

            if (listAudio.Count >= playlist.Count)
            {
                itsAll = true;
                return;
            }
           
            getLoadedTracks = true;

            Task.Run(async () =>
            {
                int offset = listAudio.Count;
                int count = 100;


                if (countTracks > listAudio.Count)
                {
                    var response = await VK.vkService.AudioGetAsync(playlist.Id, playlist.OwnerId, playlist.AccessKey, playlist.Count, count);


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
            });
        }

    }
}
