using FFMediaToolkit.Audio;
using FFMediaToolkit.Decoding;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        CancellationToken cancellationToken = default, AudioEqualizer  equalizer = null)
    {
        if (string.IsNullOrEmpty(track.Url.ToString())) return false;

        try
        {
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

            var playbackItem = await Task.Run(() =>
            {
                var file = MediaFile.Open(track.Url.ToString(), mediaOptions);
                return CreateMediaPlaybackItem(file);
            }, cancellationToken).ConfigureAwait(false);  // Добавил ConfigureAwait(false) для избежания deadlock

            // ⚠️ Устанавливаем метаданные ДО присвоения Source!
            var props = playbackItem.GetDisplayProperties();
            props.Type = MediaPlaybackType.Music;  // Обязательно!

            // Заполняем текст (проверяем на null)
            props.MusicProperties.Title = track.Title ?? "Unknown Title";
            props.MusicProperties.Artist = track.Artist ?? "Unknown Artist";  // ⚠️ Не AlbumArtist!
            props.MusicProperties.AlbumTitle = track.Album?.Title ?? "";  // Если есть альбом

            // Обложка (уже работало)
            if (track.Album?.Thumb != null)
            {
                var imageUri = track.Album.Thumb.Photo600 ??
                              track.Album.Thumb.Photo300 ??
                              track.Album.Thumb.Photo270;

                if (!string.IsNullOrEmpty(imageUri))
                {
                    try
                    {
                        props.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(imageUri));
                    }
                    catch { /* Логируем ошибку, если нужно */ }
                }
            }


            playbackItem.ApplyDisplayProperties(props);

            // Теперь присваиваем источник
            player.Source = playbackItem;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to use winrt decoder for vk media source");


            
        }
        
        return true;
        
    }
}