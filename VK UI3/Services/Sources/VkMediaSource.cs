using FFMediaToolkit.Audio;
using FFMediaToolkit.Decoding;
using MusicX.Shared.Player;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Model.Attachments;
using Windows.Media.Playback;

namespace MusicX.Services.Player.Sources;

public class VkMediaSource : MediaSourceBase
{
    private readonly Logger _logger;

    public VkMediaSource(Logger logger)
    {
        _logger = logger;
    }

    public override async Task<bool> OpenWithMediaPlayerAsync(MediaPlayer player, Audio track,
        CancellationToken cancellationToken = default, AudioEqualizer  equalizer = null)
    {
        if (string.IsNullOrEmpty(track.Url.ToString())) return false;

        try
        {
            var rtMediaSource = await CreateWinRtMediaSource(track, cancellationToken: cancellationToken, equalizer: equalizer);

            await rtMediaSource.OpenWithMediaPlayerAsync(player).AsTask(cancellationToken);
            
            RegisterSourceObjectReference(player, rtMediaSource);
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
                    mediaOptions.DemuxerOptions.PrivateOptions["af"] = $"{filterString},loudnorm=I=-16:TP=-1.5:LRA=11";
                }
            }

            // i think its better to use task.run over task.yield because we aren't doing async with ffmpeg
            var playbackItem = await Task.Run(() =>
            {
                var file = MediaFile.Open(track.Url.ToString(), mediaOptions);

                return CreateMediaPlaybackItem(file);
            }, cancellationToken);
            
            player.Source = playbackItem;
        }
        
        return true;
    }
}