using FFMediaToolkit;
using FFMediaToolkit.Audio;
using FFMediaToolkit.Decoding;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Model.Attachments;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using WinRT;

namespace MusicX.Services.Player.Sources;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class AudioEqualizer
{
    public class EqualizerBand
    {
        public string Name { get; }
        public float Frequency { get; }
        public float Width { get; }
        public float DefaultGain { get; }
        public float Gain { get; set; }
        public string WidthType { get; }

        public EqualizerBand(string name, float frequency, float width, float defaultGain, string widthType = "h")
        {
            Name = name;
            Frequency = frequency;
            Width = width;
            DefaultGain = defaultGain;
            Gain = defaultGain;
            WidthType = widthType;
        }

        public override string ToString()
        {
            return $"f={Frequency}:t={WidthType}:width={Width}:g={Gain}";
        }
    }

    public List<EqualizerBand> Bands { get; } = new List<EqualizerBand>();
    public bool IsEnabled { get; set; } = true;

    public AudioEqualizer()
    {
        InitializeDefaultBands();
    }

    private void InitializeDefaultBands()
    {
        Bands.Add(new EqualizerBand("30Hz", 30f, 25f, 0f));
        Bands.Add(new EqualizerBand("60Hz", 60f, 40f, 0f));
        Bands.Add(new EqualizerBand("80Hz", 80f, 60f, 0f));
        Bands.Add(new EqualizerBand("120Hz", 120f, 80f, 0f));
        Bands.Add(new EqualizerBand("250Hz", 250f, 100f, 0f));
        Bands.Add(new EqualizerBand("400Hz", 400f, 150f, 0f));
        Bands.Add(new EqualizerBand("600Hz", 600f, 200f, 0f));
        Bands.Add(new EqualizerBand("1kHz", 1000f, 300f, 0f));
        Bands.Add(new EqualizerBand("2.5kHz", 2500f, 600f, 0f));
        Bands.Add(new EqualizerBand("4kHz", 4000f, 1000f, 0f));
        Bands.Add(new EqualizerBand("6kHz", 6000f, 1500f, 0f));
        Bands.Add(new EqualizerBand("8kHz", 8000f, 2000f, 0f));
        Bands.Add(new EqualizerBand("10kHz", 10000f, 3000f, 0f));
        Bands.Add(new EqualizerBand("12kHz", 12000f, 4000f, 0f));
        Bands.Add(new EqualizerBand("16kHz", 16000f, 5000f, 0f));
    }

    public void SetBandGain(string name, float gain)
    {
        var band = Bands.Find(b => b.Name == name);
        if (band != null)
        {
            band.Gain = Math.Clamp(gain, -15f, 15f);
        }
    }

    public void ResetAllBands()
    {
        foreach (var band in Bands)
        {
            band.Gain = band.DefaultGain;
        }
    }

    public string GetFFmpegFilterString()
    {
        if (!IsEnabled)
            return string.Empty;

        List<string> activeFilters = new List<string>();

        foreach (var band in Bands)
        {
            if (Math.Abs(band.Gain) > 0.01f)
            {
                activeFilters.Add($"equalizer={band.ToString()}");
            }
        }

        return activeFilters.Count > 0
            ? string.Join(",", activeFilters)
            : string.Empty;
    }

    public void ApplyPreset(string presetName)
    {
        ResetAllBands();
    }
}

public abstract class MediaSourceBase : ITrackMediaSource
{
    private static readonly Semaphore FFmpegSemaphore = new(1, 1, "MusicX_FFmpegSemaphore");

    protected static readonly MediaOptions MediaOptions = new()
    {
        StreamsToLoad = MediaMode.Audio,
        AudioSampleFormat = SampleFormat.SignedWord,
        DemuxerOptions =
        {
            FlagDiscardCorrupt = false,
            FlagEnableFastSeek = true,
            SeekToAny = true,
            PrivateOptions =
            {
                ["http_persistent"] = "false",
                ["reconnect"] = "1",
                ["reconnect_streamed"] = "1",
                ["reconnect_on_network_error"] = "1",
                ["reconnect_delay_max"] = "5",
                ["reconnect_on_http_error"] = "4xx,5xx",
                ["stimeout"] = "10000000",
                ["timeout"] = "10000000",
                ["rw_timeout"] = "10000000",
                ["avioflags"] = "direct",
                ["multiple_requests"] = "1",
                ["buffer_size"] = "1024000",
                ["max_delay"] = "500000",
                ["fflags"] = "+nobuffer+fastseek",
                ["http_proxy"] = "",
                ["user_agent"] = "MusicX Player"
            }
        }
    };

    public abstract Task<bool> OpenWithMediaPlayerAsync(MediaPlayer player, Audio track, CancellationToken cancellationToken = default, AudioEqualizer equalizer = null);

    protected static MediaPlaybackItem CreateMediaPlaybackItem(MediaFile file)
    {
        System.Diagnostics.Debug.WriteLine("[FFMedia] CreateMediaPlaybackItem started");
        var streamingSource = CreateFFMediaStreamSource(file);
        var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromMediaStreamSource(streamingSource));
        System.Diagnostics.Debug.WriteLine("[FFMedia] CreateMediaPlaybackItem completed");
        return mediaPlaybackItem;
    }

    public static MediaStreamSource CreateFFMediaStreamSource(string url)
    {
        return CreateFFMediaStreamSource(MediaFile.Open(url));
    }

    public static MediaStreamSource CreateFFMediaStreamSource(MediaFile file, MediaPlayer? player = null)
    {
        if (file == null)
        {
            Debug.WriteLine("[FFMedia] MediaFile.Open вернул null.");
            throw new ArgumentNullException(nameof(file), "MediaFile.Open вернул null.");
        }

        var properties =
            AudioEncodingProperties.CreatePcm((uint)file.Audio.Info.SampleRate, (uint)file.Audio.Info.NumChannels, 16);

        var streamingSource = new MediaStreamSource(new AudioStreamDescriptor(properties))
        {
            CanSeek = true,
            IsLive = false,
            Duration = file.Audio.Info.Duration
            // BufferTime свойство не поддерживается в этой версии MediaStreamSource
        };

        var position = TimeSpan.Zero;
        var isBuffering = false;
        var lastSampleTime = DateTime.Now;
        var bufferThreshold = TimeSpan.FromSeconds(1.5);
        var consecutiveErrors = 0;
        var maxConsecutiveErrors = 3;

        streamingSource.Starting += (_, args) =>
        {
            try
            {
                position = args.Request.StartPosition == TimeSpan.Zero
                    ? file.Info.StartTime
                    : args.Request.StartPosition.GetValueOrDefault();

                Debug.WriteLine($"[FFMedia] Starting playback at position: {position}");

                args.Request.SetActualStartPosition(position);
                consecutiveErrors = 0;

                FFmpegSemaphore.WaitOne(TimeSpan.FromSeconds(10));
                try
                {
                    file.Audio.GetFrame(position);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[FFMedia] Error in Starting event: {ex.Message}");
                }
                finally
                {
                    FFmpegSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FFMedia] Starting event error: {ex.Message}");
            }
        };

        streamingSource.Closed += (_, _) =>
        {
            Debug.WriteLine("[FFMedia] MediaStreamSource closed, disposing file");
            FFmpegSemaphore.WaitOne(TimeSpan.FromSeconds(10));
            try
            {
                if (!file.IsDisposed)
                {
                    file.Dispose();
                    Debug.WriteLine("[FFMedia] File disposed successfully");
                }
                else
                {
                    Debug.WriteLine("[FFMedia] File was already disposed");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FFMedia] Error disposing file: {ex.Message}");
            }
            finally
            {
                FFmpegSemaphore.Release();
            }
        };

        streamingSource.SampleRequested += (_, args) =>
        {
            if (!FFmpegSemaphore.WaitOne(TimeSpan.FromSeconds(5)))
            {
                Debug.WriteLine("[FFMedia] Timeout waiting for FFmpeg semaphore");
                args.Request.Sample = null;
                return;
            }

            try
            {
                if (file.IsDisposed)
                {
                    Debug.WriteLine("[FFMedia] File is disposed, cannot get sample");
                    args.Request.Sample = null;
                    return;
                }

                var array = ProcessSample();
                if (array != null && array.Length > 0)
                {
                    var sample = MediaStreamSample.CreateFromBuffer(array.AsBuffer(), position);
                    var sampleDuration = CalculateSampleDuration(array.Length, properties);
                    sample.Duration = sampleDuration;

                    args.Request.Sample = sample;

                    lastSampleTime = DateTime.Now;
                    isBuffering = false;
                    consecutiveErrors = 0;

                    // Если был на паузе из-за буферизации — продолжаем воспроизведение
                    if (player?.PlaybackSession?.PlaybackState == MediaPlaybackState.Paused)
                    {
                        Debug.WriteLine("[FFMedia] Resuming playback after buffering");
                        player.Play();
                    }
                }
                else
                {
                    // Нет данных
                    args.Request.Sample = null;
                    consecutiveErrors++;

                    // Проверяем, не зависли ли мы в буферизации
                    if (DateTime.Now - lastSampleTime > bufferThreshold && !isBuffering)
                    {
                        isBuffering = true;
                        Debug.WriteLine($"[FFMedia] Buffering detected at position: {position}, consecutive errors: {consecutiveErrors}");

                        // Если слишком много ошибок подряд, пытаемся восстановиться
                        if (consecutiveErrors >= maxConsecutiveErrors)
                        {
                            Debug.WriteLine($"[FFMedia] Attempting recovery after {consecutiveErrors} consecutive errors");
                            TryRecoverFromBuffering(player, file, ref position);
                            consecutiveErrors = 0;
                        }
                    }

                    // Ставим на паузу, если проигрыватель все еще играет
                    if (player?.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing)
                    {
                        Debug.WriteLine("[FFMedia] Pausing playback due to buffering");
                        player.Pause();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FFMedia] Error in SampleRequested: {ex.Message}");
                args.Request.Sample = null;
                consecutiveErrors++;
            }
            finally
            {
                FFmpegSemaphore.Release();
            }
        };

        byte[]? ProcessSample()
        {
            AudioData frame;
            int retryCount = 0;
            const int maxRetries = 8;

            while (retryCount < maxRetries)
            {
                try
                {
                    if (!file.Audio.TryGetNextFrame(out frame))
                    {
                        // Проверяем, не достигли ли мы конца потока
                        if (file.Audio.Position >= file.Audio.Info.Duration - TimeSpan.FromMilliseconds(500))
                        {
                            Debug.WriteLine("[FFMedia] End of stream reached");
                            return null;
                        }

                        // Пробуем снова с небольшой задержкой
                        retryCount++;
                        if (retryCount < maxRetries)
                        {
                            Debug.WriteLine($"[FFMedia] No frame available, retry {retryCount}/{maxRetries}");
                            Thread.Sleep(50 * retryCount);
                            continue;
                        }

                        Debug.WriteLine("[FFMedia] Could not get frame after retries");
                        return null;
                    }

                    position = file.Audio.Position;

                    // Успешно получили кадр
                    var blockSize = frame.NumSamples * Unsafe.SizeOf<short>();
                    var array = new byte[frame.NumChannels * blockSize];

                    frame.GetChannelData<short>(0).CopyTo(MemoryMarshal.Cast<byte, short>(array));

                    frame.Dispose();

                    return array;
                }
                catch (EndOfStreamException)
                {
                    Debug.WriteLine("[FFMedia] End of stream exception");
                    return null;
                }
                catch (FFmpegException ex) when (ex.ErrorCode == -541478725) // AVERROR_EOF
                {
                    Debug.WriteLine("[FFMedia] FFmpeg EOF error");
                    return null;
                }
                catch (FFmpegException ex) when (ex.ErrorCode == -1094995529) // AVERROR(EAGAIN)
                {
                    retryCount++;
                    Debug.WriteLine($"[FFMedia] Resource temporarily unavailable, retry {retryCount}/{maxRetries}");
                    if (retryCount >= maxRetries)
                    {
                        Debug.WriteLine("[FFMedia] Max retries reached for EAGAIN");
                        return null;
                    }
                    Thread.Sleep(100 * retryCount);
                    continue;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Debug.WriteLine($"[FFMedia] Error getting frame (retry {retryCount}/{maxRetries}): {ex.Message}");
                    if (retryCount >= maxRetries)
                    {
                        Debug.WriteLine("[FFMedia] Max retries reached");
                        return null;
                    }

                    Thread.Sleep(Math.Min(1000, 150 * retryCount));
                }
            }

            return null;
        }

        return streamingSource;
    }

    private static TimeSpan CalculateSampleDuration(int bufferSize, AudioEncodingProperties properties)
    {
        try
        {
            var bytesPerSecond = properties.Bitrate / 8;
            if (bytesPerSecond > 0)
            {
                var durationSeconds = bufferSize / (double)bytesPerSecond;
                return TimeSpan.FromSeconds(durationSeconds);
            }
        }
        catch
        {
        }

        return TimeSpan.FromSeconds(0.1);
    }

    private static void TryRecoverFromBuffering(MediaPlayer? player, MediaFile file, ref TimeSpan position)
    {
        if (player == null || file == null || file.IsDisposed)
            return;

        Debug.WriteLine($"[FFMedia] Attempting recovery at position: {position}");

        try
        {
            // Пытаемся сделать seek для сброса состояния декодера
            FFmpegSemaphore.WaitOne(TimeSpan.FromSeconds(5));

            try
            {
                // Сохраняем текущую позицию
                var currentPosition = position;

                // Пытаемся получить кадр с текущей позиции
                var testFrame = file.Audio.GetFrame(currentPosition);
                testFrame.Dispose();

                Debug.WriteLine($"[FFMedia] Recovery successful with direct frame get");

                Thread.Sleep(100);
            }
            catch (Exception seekEx)
            {
                Debug.WriteLine($"[FFMedia] Direct frame get failed: {seekEx.Message}");

                // Если прямой seek не сработал, попробуем получить следующий кадр
                if (file.Audio.TryGetNextFrame(out var nextFrame))
                {
                    nextFrame.Dispose();
                    Debug.WriteLine("[FFMedia] Recovery with TryGetNextFrame successful");
                }
                else
                {
                    Debug.WriteLine("[FFMedia] Could not get next frame for recovery");
                }
            }
            finally
            {
                FFmpegSemaphore.Release();
            }

            // Даем время на восстановление сети
            Thread.Sleep(200);

            // Возобновляем воспроизведение если возможно
            if (player.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
            {
                player.Play();
                Debug.WriteLine("[FFMedia] Playback resumed after recovery attempt");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FFMedia] Recovery error: {ex.Message}");
        }
    }

    public static Task<FFmpegInteropX.FFmpegMediaSource> CreateWinRtMediaSource(Audio data, IReadOnlyDictionary<string, string>? customOptions = null, CancellationToken cancellationToken = default)
    {
        var options = new PropertySet();

        foreach (var option in MediaOptions.DemuxerOptions.PrivateOptions)
        {
            if (option.Key != null && option.Value != null)
                options.Add(option.Key, option.Value);
        }

        if (!MediaOptions.DemuxerOptions.FlagDiscardCorrupt)
            options.Add("err_detect", "ignore_err");


        if (customOptions != null)
            foreach (var (key, value) in customOptions)
                if (key != null && value != null)
                    options[key] = value;

        // Добавляем дополнительные опции для лучшего восстановления
        options["reconnect"] = "1";
        options["reconnect_delay_max"] = "5";
        options["reconnect_streamed"] = "1";
        options["stimeout"] = "10000000";

        return FFmpegInteropX.FFmpegMediaSource.CreateFromUriAsync(data.Url.ToString(), new()
        {
            FFmpegOptions = options,
            General =
            {
                ReadAheadBufferEnabled = true,
                SkipErrors = uint.MaxValue,
                KeepMetadataOnMediaSourceClosed = false
                // BufferTime не поддерживается в GeneralConfig
            }
        }).AsTask(cancellationToken);
    }

    protected static void RegisterSourceObjectReference(MediaPlayer player, IWinRTObject rtObject)
    {
        Debug.WriteLine("[FFMedia] Registering source object reference");
        GC.SuppressFinalize(rtObject.NativeObject);

        player.SourceChanged += PlayerOnSourceChanged;

        void PlayerOnSourceChanged(MediaPlayer sender, object args)
        {
            Debug.WriteLine("[FFMedia] Player source changed, disposing rtObject");
            player.SourceChanged -= PlayerOnSourceChanged;

            if (rtObject is IDisposable disposable)
            {
                disposable.Dispose();
                Debug.WriteLine("[FFMedia] rtObject disposed successfully");
            }
            else
            {
                GC.ReRegisterForFinalize(rtObject);
                Debug.WriteLine("[FFMedia] rtObject re-registered for finalization");
            }
        }
    }
}