namespace FFMediaToolkit.Decoding
{
    using System;
    using System.IO;
    using FFMediaToolkit.Audio;
    using FFMediaToolkit.Common.Internal;
    using FFMediaToolkit.Decoding.Internal;
    using FFMediaToolkit.Helpers;
    using FFmpeg.AutoGen;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an audio stream in the <see cref="MediaFile"/>.
    /// </summary>
    public unsafe class AudioStream : MediaStream
    {
        private readonly SampleFormat targetSampleFormat;
        private SwrContext* swrContext;
        private AVFilterGraph* filterGraph;
        private AVFilterContext* buffersrcCtx;
        private AVFilterContext* buffersinkCtx;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioStream"/> class.
        /// </summary>
        /// <param name="stream">The audio stream.</param>
        /// <param name="options">The decoder settings.</param>
        /// <param name="sampleFormat">The output sample format.</param>
        internal AudioStream(Decoder stream, MediaOptions options, SampleFormat sampleFormat = SampleFormat.SingleP)
            : base(stream, options)
        {
            targetSampleFormat = sampleFormat;
            var layout = Info.ChannelLayout;
            SwrContext* context;
            ffmpeg.swr_alloc_set_opts2(
                &context,
                &layout,
                (AVSampleFormat)sampleFormat,
                Info.SampleRate,
                &layout,
                (AVSampleFormat)Info.SampleFormat,
                Info.SampleRate,
                0,
                null).ThrowIfError("Cannot allocate SwrContext");
            ffmpeg.swr_init(context);
            swrContext = context;

            if (!string.IsNullOrEmpty(options.AudioFilterString))
            {
                InitFilterGraph(options.AudioFilterString);
            }
        }

        /// <summary>
        /// Gets informations about this stream.
        /// </summary>
        public new AudioStreamInfo Info => base.Info as AudioStreamInfo;

        /// <summary>
        /// Reads the next frame from the audio stream.
        /// </summary>
        /// <returns>The decoded audio data.</returns>
        public new AudioData GetNextFrame()
        {
            var frame = base.GetNextFrame() as AudioFrame;

            AudioFrame filteredFrame = frame;
            if (filterGraph != null)
            {
                filteredFrame = ApplyFilterGraph(frame);
            }

            var converted = AudioFrame.Create(
                filteredFrame.SampleRate,
                filteredFrame.NumChannels,
                filteredFrame.NumSamples,
                filteredFrame.ChannelLayout,
                targetSampleFormat,
                filteredFrame.DecodingTimestamp,
                filteredFrame.PresentationTimestamp);

            ffmpeg.swr_convert_frame(swrContext, converted.Pointer, filteredFrame.Pointer).ThrowIfError("Cannot resample frame");

            if (filteredFrame != frame)
                filteredFrame.Dispose();

            return new AudioData(converted);
        }

        /// <summary>
        /// Reads the next frame from the audio stream.
        /// A <see langword="false"/> return value indicates that reached end of stream.
        /// The method throws exception if another error has occurred.
        /// </summary>
        /// <param name="data">The decoded audio data.</param>
        /// <returns><see langword="false"/> if reached end of the stream.</returns>
        public bool TryGetNextFrame(out AudioData data)
        {
            try
            {
                data = GetNextFrame();
                return true;
            }
            catch (EndOfStreamException)
            {
                data = default;
                return false;
            }
        }

        /// <summary>
        /// Reads the video frame found at the specified timestamp.
        /// </summary>
        /// <param name="time">The frame timestamp.</param>
        /// <returns>The decoded video frame.</returns>
        public new AudioData GetFrame(TimeSpan time)
        {
            var frame = base.GetFrame(time) as AudioFrame;

            var converted = AudioFrame.Create(
                frame.SampleRate,
                frame.NumChannels,
                frame.NumSamples,
                frame.ChannelLayout,
                targetSampleFormat,
                frame.DecodingTimestamp,
                frame.PresentationTimestamp);

            ffmpeg.swr_convert_frame(swrContext, converted.Pointer, frame.Pointer).ThrowIfError("Cannot resample frame");

            return new AudioData(converted);
        }

        /// <summary>
        /// Reads the audio data found at the specified timestamp.
        /// A <see langword="false"/> return value indicates that reached end of stream.
        /// The method throws exception if another error has occurred.
        /// </summary>
        /// <param name="time">The frame timestamp.</param>
        /// <param name="data">The decoded audio data.</param>
        /// <returns><see langword="false"/> if reached end of the stream.</returns>
        public bool TryGetFrame(TimeSpan time, out AudioData data)
        {
            try
            {
                data = GetFrame(time);
                return true;
            }
            catch (EndOfStreamException)
            {
                data = default;
                return false;
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (!isDisposed)
            {
                if (swrContext != null)
                {
                    SwrContext* local = swrContext;
                    ffmpeg.swr_free(&local);
                    swrContext = null;
                }

                if (filterGraph != null)
                {
                    AVFilterGraph* local = filterGraph;
                    ffmpeg.avfilter_graph_free(&local);
                    filterGraph = null;
                }

                isDisposed = true;
            }

            base.Dispose();
        }

        /// <summary>
        /// Инициализация filtergraph для аудиофильтрации.
        /// </summary>

        private void InitFilterGraph(string filterDesc)
        {
            filterGraph = ffmpeg.avfilter_graph_alloc();
            if (filterGraph == null)
                throw new Exception("Не удалось создать AVFilterGraph");

            // buffer src
            var abuffer = ffmpeg.avfilter_get_by_name("abuffer");
            var abufferCtx = (AVFilterContext*)null;
            string sampleFmtStr = ffmpeg.av_get_sample_fmt_name((AVSampleFormat)Info.SampleFormat);
            ulong mask = Info.ChannelLayout.u.mask;
            if (mask == 0) mask = 3; // fallback: stereo
            var args = $"time_base=1/{Info.SampleRate}:sample_rate={Info.SampleRate}:sample_fmt={sampleFmtStr}:channel_layout=0x{mask:X}";
            ffmpeg.avfilter_graph_create_filter(&abufferCtx, abuffer, "in", args, null, filterGraph).ThrowIfError("Не удалось создать abuffer");
            buffersrcCtx = abufferCtx;

            // buffer sink
            var abuffersink = ffmpeg.avfilter_get_by_name("abuffersink");
            var abuffersinkCtx = (AVFilterContext*)null;
            ffmpeg.avfilter_graph_create_filter(&abuffersinkCtx, abuffersink, "out", null, null, filterGraph).ThrowIfError("Не удалось создать abuffersink");
            buffersinkCtx = abuffersinkCtx;

            // link
            AVFilterInOut* outputs = ffmpeg.avfilter_inout_alloc();
            AVFilterInOut* inputs = ffmpeg.avfilter_inout_alloc();
            outputs->name = ffmpeg.av_strdup("in");
            outputs->filter_ctx = buffersrcCtx;
            outputs->pad_idx = 0;
            outputs->next = null;

            inputs->name = ffmpeg.av_strdup("out");
            inputs->filter_ctx = buffersinkCtx;
            inputs->pad_idx = 0;
            inputs->next = null;

            ffmpeg.avfilter_graph_parse_ptr(filterGraph, filterDesc, &inputs, &outputs, null).ThrowIfError("Ошибка парсинга filtergraph");
            ffmpeg.avfilter_graph_config(filterGraph, null).ThrowIfError("Ошибка конфигурации filtergraph");

            ffmpeg.avfilter_inout_free(&inputs);
            ffmpeg.avfilter_inout_free(&outputs);
        }

        /// <summary>
        /// Применяет filtergraph к аудиофрейму.
        /// </summary>
        private AudioFrame ApplyFilterGraph(AudioFrame frame)
        {
            // Отправляем фрейм во входной фильтр
            ffmpeg.av_buffersrc_add_frame_flags(buffersrcCtx, frame.Pointer, 0).ThrowIfError("Ошибка отправки фрейма во вход фильтра");

            // Получаем обработанный фрейм в пустой AVFrame
            var filtered = AudioFrame.CreateEmpty();
            int ret = ffmpeg.av_buffersink_get_frame(buffersinkCtx, filtered.Pointer);
            if (ret < 0)
                throw new Exception($"Ошибка получения фрейма из фильтра: {ret}");

            return filtered;
        }
    }

    // --- Новый класс для потокового чтения PCM с фильтрами и поддержкой m3u8 ---
    public unsafe class StreamingAudioReader : IDisposable
    {
        private AVFormatContext* fmt_ctx;
        private AVCodecContext* codec_ctx;
        private int streamIndex;
        private AVFilterGraph* filterGraph;
        private AVFilterContext* buffersrcCtx;
        private AVFilterContext* buffersinkCtx;
        private AVPacket* pkt;
        private AVFrame* frame;
        private AVFrame* filt_frame;
        private SwrContext* swr_ctx;
        private bool disposed;
        private string _url;
        private string _filterDesc;

        public int SampleRate { get; private set; }
        public int Channels { get; private set; }
        public int BitsPerSample { get; private set; } = 16;

        public StreamingAudioReader(string url, string filterDesc = null, string referer = null)
        {
            _url = url;
            _filterDesc = filterDesc;
            ffmpeg.avformat_network_init();
            fmt_ctx = null;
            AVFormatContext* localFmtCtx = fmt_ctx;

            AVDictionary* options = null;
            ffmpeg.av_dict_set(&options, "user_agent", "Mozilla/5.0", 0);
            // Добавим referer, если требуется
            if (!string.IsNullOrEmpty(referer))
                ffmpeg.av_dict_set(&options, "referer", referer, 0);
            // Для HLS иногда требуется явно whitelist
            ffmpeg.av_dict_set(&options, "protocol_whitelist", "file,http,https,tcp,tls,crypto", 0);

            ffmpeg.avformat_open_input(&localFmtCtx, url, null, &options).ThrowIfError("open input");
            ffmpeg.av_dict_free(&options);
            fmt_ctx = localFmtCtx;
            ffmpeg.avformat_find_stream_info(fmt_ctx, null).ThrowIfError("find stream info");
            streamIndex = -1;
            for (int i = 0; i < fmt_ctx->nb_streams; i++)
            {
                if (fmt_ctx->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    streamIndex = i;
                    break;
                }
            }
            if (streamIndex == -1) throw new Exception("No audio stream found");
            var codec = ffmpeg.avcodec_find_decoder(fmt_ctx->streams[streamIndex]->codecpar->codec_id);
            codec_ctx = ffmpeg.avcodec_alloc_context3(codec);
            ffmpeg.avcodec_parameters_to_context(codec_ctx, fmt_ctx->streams[streamIndex]->codecpar).ThrowIfError("params to ctx");
            ffmpeg.avcodec_open2(codec_ctx, codec, null).ThrowIfError("open codec");
            SampleRate = codec_ctx->sample_rate;
            Channels = codec_ctx->ch_layout.nb_channels;
            // --- filtergraph ---
            if (!string.IsNullOrEmpty(filterDesc))
            {
                string finalFilterDesc = filterDesc.Trim();
                if (!finalFilterDesc.Contains("aresample"))
                {
                    if (string.IsNullOrWhiteSpace(finalFilterDesc))
                        finalFilterDesc = $"aresample=sample_rate={codec_ctx->sample_rate}";
                    else
                        finalFilterDesc += $",aresample=sample_rate={codec_ctx->sample_rate}";
                }
                // Для отладки: логируем итоговую строку фильтра
                System.Diagnostics.Debug.WriteLine($"[FFMediaToolkit] Итоговый filterDesc: {finalFilterDesc}");
                InitFilterGraph(finalFilterDesc);
            }
            pkt = ffmpeg.av_packet_alloc();
            frame = ffmpeg.av_frame_alloc();
            filt_frame = ffmpeg.av_frame_alloc();
            // --- SwrContext для преобразования в S16 interleaved ---
            SwrContext* context;
            AVChannelLayout outLayout = codec_ctx->ch_layout;
            AVChannelLayout inLayout = codec_ctx->ch_layout;
            if (outLayout.u.mask == 0) outLayout.u.mask = 3; // fallback: stereo
            if (inLayout.u.mask == 0) inLayout.u.mask = 3;
            ffmpeg.swr_alloc_set_opts2(
                &context,
                &outLayout,
                (AVSampleFormat)AVSampleFormat.AV_SAMPLE_FMT_S16,
                codec_ctx->sample_rate,
                &inLayout,
                (AVSampleFormat)codec_ctx->sample_fmt,
                codec_ctx->sample_rate,
                0,
                null).ThrowIfError("Cannot allocate SwrContext");
            ffmpeg.swr_init(context);
            swr_ctx = context;
        }

        private void InitFilterGraph(string filterDesc)
        {
            filterGraph = ffmpeg.avfilter_graph_alloc();
            if (filterGraph == null)
                throw new Exception("Не удалось создать AVFilterGraph");
            var abuffer = ffmpeg.avfilter_get_by_name("abuffer");
            var abufferCtx = (AVFilterContext*)null;
            // Получаем строку формата sample_fmt корректно для любой версии FFmpeg.AutoGen
            string sampleFmtStr;
#if NET5_0_OR_GREATER || NETCOREAPP
            // В новых версиях ffmpeg.av_get_sample_fmt_name возвращает sbyte*
            sampleFmtStr = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)ffmpeg.av_get_sample_fmt_name(codec_ctx->sample_fmt));
#else
            // В старых версиях может возвращать string
            sampleFmtStr = ffmpeg.av_get_sample_fmt_name(codec_ctx->sample_fmt);
#endif
            ulong mask = codec_ctx->ch_layout.u.mask;
            if (mask == 0) mask = 3; // fallback: stereo
            var args = $"time_base=1/{codec_ctx->sample_rate}:sample_rate={codec_ctx->sample_rate}:sample_fmt={sampleFmtStr}:channel_layout=0x{mask:X}";
            ffmpeg.avfilter_graph_create_filter(&abufferCtx, abuffer, "in", args, null, filterGraph).ThrowIfError("abuffer");
            buffersrcCtx = abufferCtx;
            var abuffersink = ffmpeg.avfilter_get_by_name("abuffersink");
            var abuffersinkCtx = (AVFilterContext*)null;
            ffmpeg.avfilter_graph_create_filter(&abuffersinkCtx, abuffersink, "out", null, null, filterGraph).ThrowIfError("abuffersink");
            buffersinkCtx = abuffersinkCtx;
            AVFilterInOut* outputs = ffmpeg.avfilter_inout_alloc();
            AVFilterInOut* inputs = ffmpeg.avfilter_inout_alloc();
            outputs->name = ffmpeg.av_strdup("in");
            outputs->filter_ctx = buffersrcCtx;
            outputs->pad_idx = 0;
            outputs->next = null;
            inputs->name = ffmpeg.av_strdup("out");
            inputs->filter_ctx = buffersinkCtx;
            inputs->pad_idx = 0;
            inputs->next = null;
            ffmpeg.avfilter_graph_parse_ptr(filterGraph, filterDesc, &inputs, &outputs, null).ThrowIfError("parse filter");
            ffmpeg.avfilter_graph_config(filterGraph, null).ThrowIfError("config filter");
            ffmpeg.avfilter_inout_free(&inputs);
            ffmpeg.avfilter_inout_free(&outputs);
        }

        public void Dispose()
        {
            if (disposed) return;

            if (codec_ctx != null)
            {
                AVCodecContext* local = codec_ctx;
                ffmpeg.avcodec_free_context(&local);
                codec_ctx = null;
            }
            if (fmt_ctx != null)
            {
                AVFormatContext* local = fmt_ctx;
                ffmpeg.avformat_close_input(&local);
                fmt_ctx = null;
            }
            if (filterGraph != null)
            {
                AVFilterGraph* local = filterGraph;
                ffmpeg.avfilter_graph_free(&local);
                filterGraph = null;
            }
            if (swr_ctx != null)
            {
                SwrContext* local = swr_ctx;
                ffmpeg.swr_free(&local);
                swr_ctx = null;
            }
            disposed = true;
        }

        public PcmChunkEnumerator CreatePcmChunkEnumerator(int chunkSize = 4096 * 4)
        {
            return new PcmChunkEnumerator(
                pkt,
                frame,
                filt_frame,
                fmt_ctx,
                codec_ctx,
                streamIndex,
                filterGraph,
                buffersrcCtx,
                buffersinkCtx,
                swr_ctx,
                chunkSize);
        }

        // Новый перегруженный метод с поддержкой стартовой позиции
        public PcmChunkEnumerator CreatePcmChunkEnumerator(TimeSpan startPosition, int chunkSize = 4096 * 4)
        {
            var enumerator = new PcmChunkEnumerator(
                pkt,
                frame,
                filt_frame,
                fmt_ctx,
                codec_ctx,
                streamIndex,
                filterGraph,
                buffersrcCtx,
                buffersinkCtx,
                swr_ctx,
                chunkSize);
            if (startPosition > TimeSpan.Zero)
            {
                enumerator.SkipUntil(startPosition);
            }
            return enumerator;
        }

        public void Seek(TimeSpan position)
        {
            // Всегда пробуем seek через FFmpeg, даже для m3u8
            long timestamp = (long)(position.TotalSeconds * fmt_ctx->streams[streamIndex]->time_base.den / fmt_ctx->streams[streamIndex]->time_base.num);
            int ret = ffmpeg.av_seek_frame(fmt_ctx, streamIndex, timestamp, ffmpeg.AVSEEK_FLAG_BACKWARD);
            if (ret < 0)
            {
                // Если не удалось — просто сбрасываем декодер и начинаем с начала
                ffmpeg.av_seek_frame(fmt_ctx, streamIndex, 0, ffmpeg.AVSEEK_FLAG_BACKWARD);
            }
            ffmpeg.avcodec_flush_buffers(codec_ctx);
        }
    }
}
