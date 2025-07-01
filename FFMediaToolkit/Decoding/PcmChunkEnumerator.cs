using System;
using System.Collections.Generic;
using FFMediaToolkit.Helpers;
using FFmpeg.AutoGen;

namespace FFMediaToolkit.Decoding
{
    public unsafe class PcmChunkEnumerator : IDisposable
    {
        private AVPacket* pkt;
        private AVFrame* frame;
        private AVFrame* filt_frame;
        private AVFormatContext* fmt_ctx;
        private AVCodecContext* codec_ctx;
        private int streamIndex;
        private AVFilterGraph* filterGraph;
        private AVFilterContext* buffersrcCtx;
        private AVFilterContext* buffersinkCtx;
        private SwrContext* swr_ctx;
        private int chunkSize;
        private List<byte> pcmBuffer;
        private bool finished;
        public byte[] Current { get; private set; }
        private AVSampleFormat lastSampleFmt = (AVSampleFormat)(-1);
        private int lastSampleRate = -1;
        private AVChannelLayout lastChLayout;
        private bool lastChLayoutValid = false;
        private double currentTimeSec = 0;

        public PcmChunkEnumerator(
            AVPacket* pkt,
            AVFrame* frame,
            AVFrame* filt_frame,
            AVFormatContext* fmt_ctx,
            AVCodecContext* codec_ctx,
            int streamIndex,
            AVFilterGraph* filterGraph,
            AVFilterContext* buffersrcCtx,
            AVFilterContext* buffersinkCtx,
            SwrContext* swr_ctx,
            int chunkSize = 4096 * 4)
        {
            this.pkt = pkt;
            this.frame = frame;
            this.filt_frame = filt_frame;
            this.fmt_ctx = fmt_ctx;
            this.codec_ctx = codec_ctx;
            this.streamIndex = streamIndex;
            this.filterGraph = filterGraph;
            this.buffersrcCtx = buffersrcCtx;
            this.buffersinkCtx = buffersinkCtx;
            this.swr_ctx = swr_ctx;
            this.chunkSize = chunkSize;
            this.pcmBuffer = new List<byte>(chunkSize);
            this.finished = false;
            this.lastChLayoutValid = false;
            this.currentTimeSec = 0;
        }

        public bool MoveNext()
        {
            if (finished) return false;
            pcmBuffer.Clear();
            while (pcmBuffer.Count < chunkSize)
            {
                if (ffmpeg.av_read_frame(fmt_ctx, pkt) < 0)
                {
                    finished = true;
                    break;
                }
                if (pkt->stream_index == streamIndex)
                {
                    int sendRet = ffmpeg.avcodec_send_packet(codec_ctx, pkt);
                    if (sendRet == ffmpeg.AVERROR(ffmpeg.EAGAIN) || sendRet == ffmpeg.AVERROR_EOF)
                    {
                        ffmpeg.av_packet_unref(pkt);
                        continue;
                    }
                    if (sendRet == ffmpeg.AVERROR_INVALIDDATA)
                    {
                        ffmpeg.av_packet_unref(pkt);
                        continue;
                    }
                    sendRet.ThrowIfError("send packet");
                    while (true)
                    {
                        int receiveRet = ffmpeg.avcodec_receive_frame(codec_ctx, frame);
                        if (receiveRet == ffmpeg.AVERROR(ffmpeg.EAGAIN) || receiveRet == ffmpeg.AVERROR_EOF)
                            break;
                        if (receiveRet == ffmpeg.AVERROR_INVALIDDATA)
                        {
                            ffmpeg.av_frame_unref(frame);
                            continue;
                        }
                        receiveRet.ThrowIfError("receive frame");
                        AVFrame* srcFrame = frame;
                        bool usedFilter = false;
                        if (filterGraph != null)
                        {
                            ffmpeg.av_buffersrc_add_frame_flags(buffersrcCtx, frame, 0).ThrowIfError("src add");
                            while (true)
                            {
                                int filterRet = ffmpeg.av_buffersink_get_frame(buffersinkCtx, filt_frame);
                                if (filterRet == ffmpeg.AVERROR(ffmpeg.EAGAIN) || filterRet == ffmpeg.AVERROR_EOF)
                                    break;
                                filterRet.ThrowIfError("sink get");
                                srcFrame = filt_frame;
                                usedFilter = true;
                                if (srcFrame->nb_samples == 0 || srcFrame->ch_layout.nb_channels == 0 || srcFrame->sample_rate == 0)
                                {
                                    ffmpeg.av_frame_unref(filt_frame);
                                    ffmpeg.av_frame_unref(frame);
                                    continue;
                                }
                                bool needReinit =
                                    srcFrame->format != (int)lastSampleFmt ||
                                    srcFrame->sample_rate != lastSampleRate ||
                                    !lastChLayoutValid;
                                if (!needReinit)
                                {
                                    AVChannelLayout srcLayout = srcFrame->ch_layout;
                                    AVChannelLayout lastLayout = lastChLayout;
                                    if (ffmpeg.av_channel_layout_compare(&srcLayout, &lastLayout) != 1)
                                        needReinit = true;
                                }
                                if (needReinit)
                                {
                                    if (swr_ctx != null)
                                    {
                                        SwrContext* local = swr_ctx;
                                        ffmpeg.swr_free(&local);
                                        swr_ctx = null;
                                    }
                                    AVChannelLayout* outLayout = stackalloc AVChannelLayout[1];
                                    AVChannelLayout* inLayout = stackalloc AVChannelLayout[1];
                                    *outLayout = srcFrame->ch_layout;
                                    *inLayout = srcFrame->ch_layout;
                                    ffmpeg.av_channel_layout_default(outLayout, srcFrame->ch_layout.nb_channels);
                                    SwrContext* newSwr = null;
                                    ffmpeg.swr_alloc_set_opts2(
                                        &newSwr,
                                        outLayout,
                                        (AVSampleFormat)AVSampleFormat.AV_SAMPLE_FMT_S16,
                                        srcFrame->sample_rate,
                                        inLayout,
                                        (AVSampleFormat)srcFrame->format,
                                        srcFrame->sample_rate,
                                        0,
                                        null).ThrowIfError("Cannot allocate SwrContext");
                                    swr_ctx = newSwr;
                                }
                                ffmpeg.swr_init(swr_ctx);
                                lastSampleFmt = (AVSampleFormat)srcFrame->format;
                                lastSampleRate = srcFrame->sample_rate;
                                lastChLayout = srcFrame->ch_layout;
                                lastChLayoutValid = true;
                                AVFrame* outFrame = ffmpeg.av_frame_alloc();
                                outFrame->nb_samples = srcFrame->nb_samples;
                                outFrame->sample_rate = srcFrame->sample_rate;
                                outFrame->format = (int)AVSampleFormat.AV_SAMPLE_FMT_S16;
                                outFrame->ch_layout = srcFrame->ch_layout;
                                outFrame->ch_layout.nb_channels = srcFrame->ch_layout.nb_channels;
                                ffmpeg.av_channel_layout_default(&outFrame->ch_layout, outFrame->ch_layout.nb_channels);
                                ffmpeg.av_frame_get_buffer(outFrame, 0).ThrowIfError("alloc outFrame");
                                ffmpeg.swr_convert_frame(swr_ctx, outFrame, srcFrame).ThrowIfError("swr_convert_frame");
                                int nb_samples = outFrame->nb_samples;
                                int ch = outFrame->ch_layout.nb_channels;
                                int bytesPerSample = ffmpeg.av_get_bytes_per_sample((AVSampleFormat)outFrame->format);
                                int len = nb_samples * ch * bytesPerSample;
                                byte[] buffer = new byte[len];
                                System.Runtime.InteropServices.Marshal.Copy((IntPtr)outFrame->data[0], buffer, 0, len);
                                pcmBuffer.AddRange(buffer);
                                // Обновляем текущее время
                                currentTimeSec += (double)nb_samples / srcFrame->sample_rate;
                                ffmpeg.av_frame_unref(filt_frame);
                                ffmpeg.av_frame_free(&outFrame);
                                if (pcmBuffer.Count >= chunkSize)
                                    break;
                            }
                        }
                        if (!usedFilter)
                        {
                            if (srcFrame->nb_samples == 0 || srcFrame->ch_layout.nb_channels == 0 || srcFrame->sample_rate == 0)
                            {
                                ffmpeg.av_frame_unref(filt_frame);
                                ffmpeg.av_frame_unref(frame);
                                continue;
                            }
                            bool needReinit2 =
                                srcFrame->format != (int)lastSampleFmt ||
                                srcFrame->sample_rate != lastSampleRate ||
                                !lastChLayoutValid;
                            if (!needReinit2)
                            {
                                AVChannelLayout srcLayout = srcFrame->ch_layout;
                                AVChannelLayout lastLayout = lastChLayout;
                                if (ffmpeg.av_channel_layout_compare(&srcLayout, &lastLayout) != 1)
                                    needReinit2 = true;
                            }
                            if (needReinit2)
                            {
                                if (swr_ctx != null)
                                {
                                    SwrContext* local = swr_ctx;
                                    ffmpeg.swr_free(&local);
                                    swr_ctx = null;
                                }
                                AVChannelLayout* outLayout = stackalloc AVChannelLayout[1];
                                AVChannelLayout* inLayout = stackalloc AVChannelLayout[1];
                                *outLayout = srcFrame->ch_layout;
                                *inLayout = srcFrame->ch_layout;
                                ffmpeg.av_channel_layout_default(outLayout, srcFrame->ch_layout.nb_channels);
                                SwrContext* newSwr = null;
                                ffmpeg.swr_alloc_set_opts2(
                                    &newSwr,
                                    outLayout,
                                    (AVSampleFormat)AVSampleFormat.AV_SAMPLE_FMT_S16,
                                    srcFrame->sample_rate,
                                    inLayout,
                                    (AVSampleFormat)srcFrame->format,
                                    srcFrame->sample_rate,
                                    0,
                                    null).ThrowIfError("Cannot allocate SwrContext");
                                swr_ctx = newSwr;
                            }
                            ffmpeg.swr_init(swr_ctx);
                            lastSampleFmt = (AVSampleFormat)srcFrame->format;
                            lastSampleRate = srcFrame->sample_rate;
                            lastChLayout = srcFrame->ch_layout;
                            lastChLayoutValid = true;
                            AVFrame* outFrame = ffmpeg.av_frame_alloc();
                            outFrame->nb_samples = srcFrame->nb_samples;
                            outFrame->sample_rate = srcFrame->sample_rate;
                            outFrame->format = (int)AVSampleFormat.AV_SAMPLE_FMT_S16;
                            outFrame->ch_layout = srcFrame->ch_layout;
                            outFrame->ch_layout.nb_channels = srcFrame->ch_layout.nb_channels;
                            ffmpeg.av_channel_layout_default(&outFrame->ch_layout, outFrame->ch_layout.nb_channels);
                            ffmpeg.av_frame_get_buffer(outFrame, 0).ThrowIfError("alloc outFrame");
                            ffmpeg.swr_convert_frame(swr_ctx, outFrame, srcFrame).ThrowIfError("swr_convert_frame");
                            int nb_samples = outFrame->nb_samples;
                            int ch = outFrame->ch_layout.nb_channels;
                            int bytesPerSample = ffmpeg.av_get_bytes_per_sample((AVSampleFormat)outFrame->format);
                            int len = nb_samples * ch * bytesPerSample;
                            byte[] buffer = new byte[len];
                            System.Runtime.InteropServices.Marshal.Copy((IntPtr)outFrame->data[0], buffer, 0, len);
                            pcmBuffer.AddRange(buffer);
                            // Обновляем текущее время
                            currentTimeSec += (double)nb_samples / srcFrame->sample_rate;
                            if (srcFrame->nb_samples == 0 || srcFrame->ch_layout.nb_channels == 0 || srcFrame->sample_rate == 0)
                            {
                                ffmpeg.av_frame_unref(filt_frame);
                                ffmpeg.av_frame_free(&outFrame);
                            }
                        }
                        ffmpeg.av_frame_unref(frame);
                        if (pcmBuffer.Count >= chunkSize)
                            break;
                    }
                }
                ffmpeg.av_packet_unref(pkt);
            }
            if (pcmBuffer.Count > 0)
            {
                Current = pcmBuffer.ToArray();
                return true;
            }
            else
            {
                Current = null;
                return false;
            }
        }

        // Метод для пропуска данных до нужной позиции (секунды)
        public void SkipUntil(TimeSpan position)
        {
            double targetSec = position.TotalSeconds;
            // Сбрасываем буфер и текущую временную метку
            pcmBuffer.Clear();
            Current = null;
            // Продвигаемся по чанкам, пока не достигнем нужного времени
            while (currentTimeSec < targetSec)
            {
                if (!MoveNext())
                    break;
                // Оцениваем длительность текущего чанка
                int bytesPerSample = 2; // S16
                int channels = codec_ctx->ch_layout.nb_channels;
                int samples = Current != null ? Current.Length / (channels * bytesPerSample) : 0;
                double chunkDuration = samples > 0 ? (double)samples / codec_ctx->sample_rate : 0;
                currentTimeSec += chunkDuration;
                // Если после этого мы достигли или превысили нужное время — выходим
                if (currentTimeSec >= targetSec)
                    break;
            }
            // После выхода: если мы превысили targetSec, то текущий чанк начинается чуть позже, чем нужно,
            // но это поведение аналогично GetFrame(time) в FFMediaToolkit (он тоже возвращает ближайший доступный фрейм)
        }

        public void Dispose() { }
    }
}
