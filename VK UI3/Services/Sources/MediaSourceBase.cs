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
            return $"f={Frequency}:t={WidthType}:w={Width}:g={Gain}";
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
        // Более оптимальные значения ширины полос
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

        List<string> filters = new List<string>();
        foreach (var band in Bands)
        {
            if (Math.Abs(band.Gain) > 0.01f)
            {
                filters.Add(band.ToString());
            }
        }

        // Для нескольких полос: equalizer=...,equalizer=...
        return filters.Count > 0 ? string.Join(",", filters.Select(f => $"equalizer={f}")) : string.Empty;
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
            FlagDiscardCorrupt = true,
            FlagEnableFastSeek = true,
            SeekToAny = true,
            PrivateOptions =
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



    public abstract Task<bool> OpenWithMediaPlayerAsync(MediaPlayer player, Audio track, CancellationToken cancellationToken = default, AudioEqualizer equalizer = null);

    protected static MediaPlaybackItem CreateMediaPlaybackItem(MediaFile file)
    {
        var streamingSource = CreateFFMediaStreamSource(file);

        return new (MediaSource.CreateFromMediaStreamSource(streamingSource));
    }



    public static MediaStreamSource CreateFFMediaStreamSource(string url)
    {
        return CreateFFMediaStreamSource(MediaFile.Open(url));
    }

    public static MediaStreamSource CreateFFMediaStreamSource(MediaFile file)
    {
        if (file == null)
        {
            // Можно заменить на ваш логгер, если он есть в статическом контексте
            System.Diagnostics.Debug.WriteLine("[FFMedia] MediaFile.Open вернул null. Возможно, ошибка в фильтре эквалайзера или FFmpeg.");
            throw new ArgumentNullException(nameof(file), "MediaFile.Open вернул null. Возможно, ошибка в фильтре эквалайзера или FFmpeg.");
        }

        var properties =
            AudioEncodingProperties.CreatePcm((uint)file.Audio.Info.SampleRate, (uint)file.Audio.Info.NumChannels, 16);

        var streamingSource = new MediaStreamSource(new AudioStreamDescriptor(properties))
        {
            CanSeek = true,
            IsLive = true,
            Duration = file.Audio.Info.Duration
        };
        
        var position = TimeSpan.Zero;

        streamingSource.Starting += (_, args) =>
        {
            position = args.Request.StartPosition == TimeSpan.Zero
                ? file.Info.StartTime
                : args.Request.StartPosition.GetValueOrDefault();
            
            args.Request.SetActualStartPosition(position);

            try
            {
                FFmpegSemaphore.WaitOne(TimeSpan.FromSeconds(10));
                file.Audio.GetFrame(position);
            }
            catch (FFmpegException)
            {
            }
            finally
            {
                FFmpegSemaphore.Release();
            }
        };

        streamingSource.Closed += (_, _) =>
        {
            FFmpegSemaphore.WaitOne(TimeSpan.FromSeconds(10));
            try
            {
                file.Dispose();
            }
            finally
            {
                FFmpegSemaphore.Release();
            }
        };

        streamingSource.SampleRequested += (_, args) =>
        {
            FFmpegSemaphore.WaitOne(TimeSpan.FromSeconds(10));
            
            try
            {
                if (file.IsDisposed)
                    return;
                
                var array = ProcessSample();
                if (array != null)
                    args.Request.Sample = MediaStreamSample.CreateFromBuffer(array.AsBuffer(), position);
            }
            finally
            {
                FFmpegSemaphore.Release();
            }
        };
        
        byte[]? ProcessSample()
        {
            AudioData frame;
            while (true)
            {
                try
                {
                    if (!file.Audio.TryGetNextFrame(out frame))
                        return null;

                    position = file.Audio.Position;
                    
                    break;
                }
                catch (FFmpegException)
                {
                }
            }

            var blockSize = frame.NumSamples * Unsafe.SizeOf<short>();
            var array = new byte[frame.NumChannels * blockSize];

            frame.GetChannelData<short>(0).CopyTo(MemoryMarshal.Cast<byte, short>(array));
            
            frame.Dispose();

            return array;
        }

        return streamingSource;
    }


    protected static void RegisterSourceObjectReference(MediaPlayer player, IWinRTObject rtObject)
    {
        GC.SuppressFinalize(rtObject.NativeObject);
        
        player.SourceChanged += PlayerOnSourceChanged;

        void PlayerOnSourceChanged(MediaPlayer sender, object args)
        {
            player.SourceChanged -= PlayerOnSourceChanged;
            
            if (rtObject is IDisposable disposable)
                disposable.Dispose();
            else
                GC.ReRegisterForFinalize(rtObject);
        }
    }
}