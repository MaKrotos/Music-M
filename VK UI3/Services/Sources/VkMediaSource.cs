using FFMediaToolkit.Audio;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Interop;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Model.Attachments;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using VK_UI3.Controllers; // Для доступа к CustomMediaPlayer

namespace MusicX.Services.Player.Sources;

public class VkMediaSource : MediaSourceBase
{
    private readonly Logger _logger;

    public VkMediaSource(Logger logger)
    {
        _logger = logger;
    }
    public override async Task<bool> OpenWithMediaPlayerAsync(CustomMediaPlayer player, Audio track,
    CancellationToken cancellationToken = default, AudioEqualizer equalizer = null)
    {
        if (string.IsNullOrEmpty(track.Url.ToString()))
            return false;
        try
        {
            // Формируем строку фильтра
            string filterString = null;
            if (equalizer != null)
            {
                filterString = equalizer.GetFFmpegFilterString();
            }

            // 1. Создаём StreamingAudioReader (он сам применяет фильтр)
            var reader = new StreamingAudioReader(track.Url.ToString(), filterString);

            // 2. Создаём MediaStreamSource с нужными параметрами
            var audioProps = Windows.Media.MediaProperties.AudioEncodingProperties.CreatePcm(
                (uint)reader.SampleRate, (ushort)reader.Channels, (ushort)reader.BitsPerSample);
            var desc = new Windows.Media.Core.AudioStreamDescriptor(audioProps);
            var mss = new Windows.Media.Core.MediaStreamSource(desc);

            // 3. Готовим enumerator для PCM-чанков
            var chunkEnumerator = reader.CreatePcmChunkEnumerator();

            TimeSpan currentTimestamp = TimeSpan.Zero;

            // Подписка на события перемотки
            player.PositionChanging += (s, newPos) =>
            {
                // Перемотка через FFmpeg
                reader.Seek(newPos);
                chunkEnumerator = reader.CreatePcmChunkEnumerator(newPos);
                currentTimestamp = newPos;
                System.Diagnostics.Debug.WriteLine($"[CustomMediaPlayer] Position changing to: {newPos}");
            };
            player.PositionChanged += (s, newPos) =>
            {
                System.Diagnostics.Debug.WriteLine($"[CustomMediaPlayer] Position changed to: {newPos}");
            };

            mss.Starting += (s, e) =>
            {
                TimeSpan pos = e.Request.StartPosition ?? new TimeSpan(0);
                reader.Seek(pos);
                chunkEnumerator = reader.CreatePcmChunkEnumerator(pos);
                currentTimestamp = pos;
                e.Request.SetActualStartPosition(pos);
            };

            mss.SampleRequested += (s, e) =>
            {
                if (chunkEnumerator.MoveNext())
                {
                    var buffer = chunkEnumerator.Current;
                    int bytesPerSample = reader.BitsPerSample / 8;
                    int samples = buffer.Length / (reader.Channels * bytesPerSample);
                    double seconds = (double)samples / reader.SampleRate;
                    var sample = Windows.Media.Core.MediaStreamSample.CreateFromBuffer(BufferFromArray(buffer), currentTimestamp);
                    e.Request.Sample = sample;
                    currentTimestamp += TimeSpan.FromSeconds(seconds);
                }
                else
                {
                    e.Request.Sample = null;
                }
            };

            // 4. Передаём MediaStreamSource в MediaPlayer
            player.InnerPlayer.Source = Windows.Media.Core.MediaSource.CreateFromMediaStreamSource(mss);

            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to use streaming reader for vk media source");
        }
        return false;
    }

    // Вспомогательная функция для создания IBuffer из byte[]
    private static Windows.Storage.Streams.IBuffer BufferFromArray(byte[] data)
    {
        var writer = new Windows.Storage.Streams.DataWriter();
        writer.WriteBytes(data);
        return writer.DetachBuffer();
    }
}