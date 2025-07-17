using FFMediaToolkit.Audio;
using FFMediaToolkit.Decoding;
using MusicX.Shared.Player;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TagLib.Mpeg;
using VK_UI3;
using VkNet.Model.Attachments;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace MusicX.Services.Player.Sources;

public class VkMediaSource : MediaSourceBase
{
    private readonly Logger _logger;

    public VkMediaSource(Logger logger)
    {
        _logger = logger;
    }
    public override async Task<bool> OpenWithMediaPlayerAsync(MediaPlayer player, Audio track,
        CancellationToken cancellationToken = default, AudioEqualizer equalizer = null)
    {
        if (string.IsNullOrEmpty(track.Url.ToString())) return false;

        try
        {
            var rtMediaSource = await CreateWinRtMediaSource(track, cancellationToken: cancellationToken);

            var eq = "";
            if (equalizer != null)
            {
                string filterString = equalizer.GetFFmpegFilterString();
                if (!string.IsNullOrEmpty(filterString))
                {
                    eq = $"{filterString}";
                }
            }   

            

            await rtMediaSource.OpenWithMediaPlayerAsync(player).AsTask(cancellationToken);

            var playbackItem = player.Source as MediaPlaybackItem;

                        MediaItemDisplayProperties props = playbackItem.GetDisplayProperties();
            props.Type = Windows.Media.MediaPlaybackType.Music;
            props.MusicProperties.Title = track.Title;
            props.MusicProperties.AlbumArtist = track.Artist;


            if (track.Album != null && track.Album.Thumb != null)
            {
                RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromUri(new Uri(
                    track.Album.Thumb.Photo600 ??
                    track.Album.Thumb.Photo270 ??
                    track.Album.Thumb.Photo300
                    ));

                props.Thumbnail = imageStreamRef;
                playbackItem.ApplyDisplayProperties(props);

                props.Thumbnail = imageStreamRef;

            }
            else
                props.Thumbnail = null;
            playbackItem.ApplyDisplayProperties(props);


            RegisterSourceObjectReference(player, rtMediaSource);

            if (!string.IsNullOrEmpty(eq) || equalizer == null)
            {
                rtMediaSource.SetFFmpegAudioFilters(eq, rtMediaSource.AudioStreams.First());
            }
            else
            {
                rtMediaSource.ClearFFmpegAudioFilters();
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to use winrt decoder for vk media source");


            var mediaOptions = new MediaOptions
            {
                StreamsToLoad = MediaMode.Audio,
                AudioSampleFormat = SampleFormat.SignedWord,
                DemuxerOptions =
            {
                FlagDiscardCorrupt = true,
                FlagEnableFastSeek = true,
                SeekToAny = true,
                PrivateOptions = new Dictionary<string, string>
                {
                    ["http_persistent"] = "false",
                    ["reconnect"] = "1",
                    ["reconnect_streamed"] = "1",
                    ["reconnect_on_network_error"] = "1",
                    ["reconnect_delay_max"] = "30",
                    ["reconnect_on_http_error"] = "4xx,5xx",
                    ["stimeout"] = "30000000",
                    ["timeout"] = "30000000",
                    ["rw_timeout"] = "30000000"
                }
            }
            };



            // Добавляем эквалайзер, если он передан
            if (equalizer != null)
            {
                string filterString = equalizer.GetFFmpegFilterString();
                if (!string.IsNullOrEmpty(filterString))
                {
                    // Добавляем loudnorm после эквалайзера
                    mediaOptions.EqualizerArgs = $"{filterString}";
                }
            }

            // i think its better to use task.run over task.yield because we aren't doing async with ffmpeg
            var playbackItem = await Task.Run(() =>
            {
                var file = MediaFile.Open(track.Url.ToString(), mediaOptions);

                return CreateMediaPlaybackItem(file);
            }, cancellationToken);

            MediaItemDisplayProperties props = playbackItem.GetDisplayProperties();
            props.Type = Windows.Media.MediaPlaybackType.Music;
            props.MusicProperties.Title = track.Title;
            props.MusicProperties.AlbumArtist = track.Artist;


            if (track.Album != null && track.Album.Thumb != null)
            {
                RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromUri(new Uri(
                    track.Album.Thumb.Photo600 ??
                    track.Album.Thumb.Photo270 ??
                    track.Album.Thumb.Photo300
                    ));

                props.Thumbnail = imageStreamRef;
                playbackItem.ApplyDisplayProperties(props);

                props.Thumbnail = imageStreamRef;

            }
            else
                props.Thumbnail = null;
            playbackItem.ApplyDisplayProperties(props);



            player.Source = playbackItem;
        }

        return true;
    }
}